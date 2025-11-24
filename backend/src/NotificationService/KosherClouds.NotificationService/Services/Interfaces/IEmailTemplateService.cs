using KosherClouds.Contracts.Orders;

namespace KosherClouds.NotificationService.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        string GenerateOrderConfirmationEmail(
            string userName,
            Guid orderId,
            List<OrderItemInfo> items,
            decimal totalAmount,
            DateTimeOffset orderDate);

        string GenerateWelcomeEmail(string userName);

        string GenerateBookingConfirmationEmail(
            string userName,
            Guid bookingId,
            DateTime bookingDateTime);

        string GeneratePasswordResetEmail(
            string userName,
            string resetToken,
            string email,
            DateTime expiresAt);
    }
}