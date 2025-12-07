using KosherClouds.ReviewService.Data;
using KosherClouds.ReviewService.Data.Seed;
using KosherClouds.ReviewService.Services;
using KosherClouds.ReviewService.Services.External;
using KosherClouds.ReviewService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Handlers;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReviewDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>((serviceProvider, client) =>
{
    var orderServiceUrl = builder.Configuration["ServiceUrls:OrderService"]
        ?? "http://localhost:5005";
    client.BaseAddress = new Uri(orderServiceUrl);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IUserApiClient, UserApiClient>((serviceProvider, client) =>
{
    var userServiceUrl = builder.Configuration["ServiceUrls:UserService"]
        ?? "http://localhost:5002";
    client.BaseAddress = new Uri(userServiceUrl);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
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
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt("KosherClouds ReviewService API");

var app = builder.Build();

await ReviewSeeder.SeedAsync(app.Services);

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

#pragma warning disable S1118
public partial class Program { }
#pragma warning restore S1118