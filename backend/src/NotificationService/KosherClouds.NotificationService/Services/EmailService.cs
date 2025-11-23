using KosherClouds.NotificationService.Configuration;
using KosherClouds.NotificationService.Models;
using KosherClouds.NotificationService.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KosherClouds.NotificationService.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mimeMessage = CreateMimeMessage(message);

                using var smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                    cancellationToken);

                if (!string.IsNullOrEmpty(_emailSettings.Username) &&
                    !string.IsNullOrEmpty(_emailSettings.Password))
                {
                    await smtpClient.AuthenticateAsync(
                        _emailSettings.Username,
                        _emailSettings.Password,
                        cancellationToken);
                }

                await smtpClient.SendAsync(mimeMessage, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation(
                    "Email sent successfully to {To} with subject '{Subject}'",
                    message.To,
                    message.Subject);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to send email to {message.To} with subject '{message.Subject}'",
                    ex);
            }
        }

        public async Task SendBulkEmailAsync(
            List<EmailMessage> messages,
            CancellationToken cancellationToken = default)
        {
            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(_emailSettings.Username) &&
                !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await smtpClient.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password,
                    cancellationToken);
            }

            foreach (var message in messages)
            {
                try
                {
                    var mimeMessage = CreateMimeMessage(message);
                    await smtpClient.SendAsync(mimeMessage, cancellationToken);

                    _logger.LogInformation(
                        "Bulk email sent successfully to {To}",
                        message.To);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send bulk email to {To}",
                        message.To);
                }
            }

            await smtpClient.DisconnectAsync(true, cancellationToken);
        }

        private MimeMessage CreateMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail));

            mimeMessage.To.Add(MailboxAddress.Parse(message.To));

            if (message.Cc != null && message.Cc.Any())
            {
                foreach (var cc in message.Cc)
                {
                    mimeMessage.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            if (message.Bcc != null && message.Bcc.Any())
            {
                foreach (var bcc in message.Bcc)
                {
                    mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc));
                }
            }

            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();

            if (message.IsHtml)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }

            if (message.Attachments != null && message.Attachments.Any())
            {
                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add(
                        attachment.Key,
                        attachment.Value);
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            return mimeMessage;
        }
    }
}
