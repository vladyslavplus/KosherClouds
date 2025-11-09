using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.Data.Seed;
using KosherClouds.ProductService.Services;
using KosherClouds.ProductService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(ProductService).Assembly);

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
    });
});

builder.Services.AddScoped(typeof(ISortHelper<>), typeof(SortHelper<>));
builder.Services.AddSingleton<ISortHelperFactory, SortHelperFactory>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt("KosherClouds ProductService API");

var app = builder.Build();

await ProductSeeder.SeedAsync(app.Services);

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