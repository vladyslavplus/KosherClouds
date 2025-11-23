using KosherClouds.Contracts.Orders;
using KosherClouds.NotificationService.Services.Interfaces;
using System.Text;

namespace KosherClouds.NotificationService.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateOrderConfirmationEmail(
            string userName,
            Guid orderId,
            List<OrderItemInfo> items,
            decimal totalAmount,
            DateTimeOffset orderDate)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("        .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine("        .content { padding: 20px; background-color: #f9f9f9; }");
            sb.AppendLine("        .order-details { background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }");
            sb.AppendLine("        .item { padding: 10px; border-bottom: 1px solid #eee; }");
            sb.AppendLine("        .item:last-child { border-bottom: none; }");
            sb.AppendLine("        .total { font-size: 18px; font-weight: bold; margin-top: 15px; padding-top: 15px; border-top: 2px solid #4CAF50; }");
            sb.AppendLine("        .footer { text-align: center; padding: 20px; color: #777; font-size: 12px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1>Order Confirmation</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='content'>");
            sb.AppendLine($"            <p>Dear {userName},</p>");
            sb.AppendLine("            <p>Thank you for your order! We're excited to prepare your delicious kosher meal.</p>");
            sb.AppendLine("            <div class='order-details'>");
            sb.AppendLine($"                <p><strong>Order ID:</strong> {orderId}</p>");
            sb.AppendLine($"                <p><strong>Order Date:</strong> {orderDate:MMMM dd, yyyy HH:mm}</p>");
            sb.AppendLine("                <h3>Order Items:</h3>");

            foreach (var item in items)
            {
                sb.AppendLine("                <div class='item'>");
                sb.AppendLine($"                    <strong>{item.ProductName}</strong><br>");
                sb.AppendLine($"                    Quantity: {item.Quantity} × ${item.UnitPrice:F2} = <strong>${item.LineTotal:F2}</strong>");
                sb.AppendLine("                </div>");
            }

            sb.AppendLine($"                <div class='total'>Total Amount: ${totalAmount:F2}</div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <p>Your order is being prepared and will be ready soon. We'll notify you once it's ready for pickup or delivery.</p>");
            sb.AppendLine("            <p>If you have any questions, please don't hesitate to contact us.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>With respect,<br><strong>Kosher Clouds Team</strong></p>");
            sb.AppendLine("            <p>This is an automated message, please do not reply to this email.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
