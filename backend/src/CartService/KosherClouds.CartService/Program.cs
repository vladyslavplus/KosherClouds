using KosherClouds.CartService.Services;
using KosherClouds.CartService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedisCache(builder.Configuration);

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt("KosherClouds CartService API");

var app = builder.Build();

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