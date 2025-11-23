using KosherClouds.Contracts.Orders;
using KosherClouds.NotificationService.Models;
using KosherClouds.NotificationService.Services.External;
using KosherClouds.NotificationService.Services.Interfaces;
using MassTransit;

namespace KosherClouds.NotificationService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IUserApiClient _userApiClient;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(
            IEmailService emailService,
            IUserApiClient userApiClient,
            IEmailTemplateService emailTemplateService,
            ILogger<OrderCreatedConsumer> logger)
        {
            _emailService = emailService;
            _userApiClient = userApiClient;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var orderEvent = context.Message;

            _logger.LogInformation(
                "Processing OrderCreatedEvent for Order {OrderId}, User {UserId}",
                orderEvent.OrderId,
                orderEvent.UserId);

            try
            {
                var user = await _userApiClient.GetUserByIdAsync(
                    orderEvent.UserId,
                    context.CancellationToken);

                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning(
                        "User {UserId} not found or has no email. Skipping notification.",
                        orderEvent.UserId);
                    return;
                }

                var userName = !string.IsNullOrWhiteSpace(user.FirstName)
                    ? user.FirstName
                    : "Valued Customer";

                var emailBody = _emailTemplateService.GenerateOrderConfirmationEmail(
                    userName,
                    orderEvent.OrderId,
                    orderEvent.Items,
                    orderEvent.TotalAmount,
                    orderEvent.CreatedAt);

                var emailMessage = new EmailMessage
                {
                    To = user.Email,
                    Subject = $"Order Confirmation #{orderEvent.OrderId.ToString()[..8]}",
                    Body = emailBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailMessage, context.CancellationToken);

                _logger.LogInformation(
                    "Order confirmation email sent to {Email} for Order {OrderId}",
                    user.Email,
                    orderEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send order confirmation email for Order {OrderId}",
                    orderEvent.OrderId);
            }
        }
    }
}
