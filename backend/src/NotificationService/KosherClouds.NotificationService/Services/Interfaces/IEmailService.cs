using KosherClouds.NotificationService.Models;

namespace KosherClouds.NotificationService.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
        Task SendBulkEmailAsync(List<EmailMessage> messages, CancellationToken cancellationToken = default);
    }
}
