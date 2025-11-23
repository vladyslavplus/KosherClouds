using KosherClouds.Contracts.Users;
using KosherClouds.NotificationService.Models;
using KosherClouds.NotificationService.Services.Interfaces;
using MassTransit;

namespace KosherClouds.NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<UserRegisteredConsumer> logger)
        {
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var userEvent = context.Message;

            _logger.LogInformation(
                "Processing UserRegisteredEvent for User {UserId}, Email {Email}",
                userEvent.UserId,
                userEvent.Email);

            try
            {
                if (string.IsNullOrEmpty(userEvent.Email))
                {
                    _logger.LogWarning(
                        "User {UserId} has no email. Skipping welcome notification.",
                        userEvent.UserId);
                    return;
                }
                var userName = !string.IsNullOrWhiteSpace(userEvent.UserName)
                    ? userEvent.UserName
                    : "Valued Customer";

                var emailBody = _emailTemplateService.GenerateWelcomeEmail(userName);

                var emailMessage = new EmailMessage
                {
                    To = userEvent.Email,
                    Subject = "Welcome to Kosher Clouds! 🎉",
                    Body = emailBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailMessage, context.CancellationToken);

                _logger.LogInformation(
                    "Welcome email sent to {Email} for User {UserId}",
                    userEvent.Email,
                    userEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send welcome email for User {UserId}",
                    userEvent.UserId);
            }
        }
    }
}