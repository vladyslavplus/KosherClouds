using KosherClouds.NotificationService.Configuration;
using KosherClouds.NotificationService.Consumers;
using KosherClouds.NotificationService.Services;
using KosherClouds.NotificationService.Services.External;
using KosherClouds.NotificationService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddHttpClient<IUserApiClient, UserApiClient>((serviceProvider, client) =>
{
    var userServiceUrl = builder.Configuration["ServiceUrls:UserService"]
        ?? "http://localhost:5002";
    client.BaseAddress = new Uri(userServiceUrl);
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<BookingCreatedConsumer>();
    x.AddConsumer<PasswordResetRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"]!);
            h.Password(builder.Configuration["RabbitMq:Password"]!);
        });

        cfg.ReceiveEndpoint("notification-order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-user-registered-queue", e =>
        {
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-booking-created-queue", e =>
        {
            e.ConfigureConsumer<BookingCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-password-reset-queue", e =>
        {
            e.ConfigureConsumer<PasswordResetRequestedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseGlobalExceptionHandler();
await app.RunAsync();