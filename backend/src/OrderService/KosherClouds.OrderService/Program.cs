using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Data.Seed;
using KosherClouds.OrderService.Handlers;
using KosherClouds.OrderService.Services;
using KosherClouds.OrderService.Services.External;
using KosherClouds.OrderService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ICartApiClient, CartApiClient>((serviceProvider, client) =>
{
    var cartServiceUrl = builder.Configuration["ServiceUrls:CartService"]
        ?? "http://localhost:5004";
    client.BaseAddress = new Uri(cartServiceUrl);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>((serviceProvider, client) =>
{
    var productServiceUrl = builder.Configuration["ServiceUrls:ProductService"]
        ?? "http://localhost:5003";
    client.BaseAddress = new Uri(productServiceUrl);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddAutoMapper(typeof(OrderService).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]!);
            h.Password(builder.Configuration["RabbitMq:Password"]!);
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped(typeof(ISortHelper<>), typeof(SortHelper<>));
builder.Services.AddSingleton<ISortHelperFactory, SortHelperFactory>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt("KosherClouds OrderService API");

var app = builder.Build();

await OrderSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();