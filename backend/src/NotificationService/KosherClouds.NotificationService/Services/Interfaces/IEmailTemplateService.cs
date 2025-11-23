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
    }
}
