using KosherClouds.Contracts.Users;
using KosherClouds.NotificationService.Models;
using KosherClouds.NotificationService.Services.Interfaces;
using MassTransit;

namespace KosherClouds.NotificationService.Consumers
{
    public class PasswordResetRequestedConsumer : IConsumer<PasswordResetRequestedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<PasswordResetRequestedConsumer> _logger;

        public PasswordResetRequestedConsumer(
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<PasswordResetRequestedConsumer> logger)
        {
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PasswordResetRequestedEvent> context)
        {
            var resetEvent = context.Message;

            _logger.LogInformation(
                "Processing PasswordResetRequestedEvent for User {UserId}, Email {Email}",
                resetEvent.UserId,
                resetEvent.Email);

            if (string.IsNullOrEmpty(resetEvent.Email))
            {
                _logger.LogWarning(
                    "User {UserId} has no email. Skipping password reset notification.",
                    resetEvent.UserId);
                return;
            }

            var emailBody = _emailTemplateService.GeneratePasswordResetEmail(
                resetEvent.UserName,
                resetEvent.ResetToken,
                resetEvent.Email,
                resetEvent.ExpiresAt);

            var emailMessage = new EmailMessage
            {
                To = resetEvent.Email,
                Subject = "Password Reset Request - Kosher Clouds",
                Body = emailBody,
                IsHtml = true
            };

            await _emailService.SendEmailAsync(emailMessage, context.CancellationToken);

            _logger.LogInformation(
                "Password reset email sent to {Email} for User {UserId}",
                resetEvent.Email,
                resetEvent.UserId);
        }
    }
}