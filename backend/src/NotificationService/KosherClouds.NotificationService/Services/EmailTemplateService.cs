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
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"            <p>Dear {userName},</p>");
            contentBuilder.AppendLine("            <p>Thank you for your order! We're excited to prepare your delicious kosher meal.</p>");
            contentBuilder.AppendLine("            <div class='order-details'>");
            contentBuilder.AppendLine($"                <p><strong>Order ID:</strong> {orderId}</p>");
            contentBuilder.AppendLine($"                <p><strong>Order Date:</strong> {orderDate:MMMM dd, yyyy HH:mm}</p>");
            contentBuilder.AppendLine("                <h3>Order Items:</h3>");

            foreach (var item in items)
            {
                contentBuilder.AppendLine("                <div class='item'>");
                contentBuilder.AppendLine($"                    <strong>{item.ProductName}</strong><br>");
                contentBuilder.AppendLine($"                    Quantity: {item.Quantity} × {item.UnitPrice:F2}₴ = <strong>{item.LineTotal:F2}₴</strong>");
                contentBuilder.AppendLine("                </div>");
            }

            contentBuilder.AppendLine($"                <div class='total'>Total Amount: {totalAmount:F2}₴</div>");
            contentBuilder.AppendLine("            </div>");
            contentBuilder.AppendLine("            <p>Your order is being prepared and will be ready soon. We'll notify you once it's ready for pickup.</p>");
            contentBuilder.AppendLine("            <p>If you have any questions, please don't hesitate to contact us.</p>");

            return GenerateEmailTemplate("Order Confirmation", contentBuilder.ToString());
        }

        public string GenerateWelcomeEmail(string userName)
        {
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"            <p>Dear {userName},</p>");
            contentBuilder.AppendLine("            <p>Welcome to <strong>Kosher Clouds</strong>!</p>");
            contentBuilder.AppendLine("            <p>We're thrilled to have you join our community. At Kosher Clouds, we're dedicated to bringing you the finest kosher cuisine with a modern twist.</p>");
            contentBuilder.AppendLine("            <div class='order-details'>");
            contentBuilder.AppendLine("                <h3>What's Next?</h3>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>🍽️ Browse Our Menu</strong><br>");
            contentBuilder.AppendLine("                    Discover our wide selection of delicious kosher dishes prepared with the finest ingredients.");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>🛒 Place Your First Order</strong><br>");
            contentBuilder.AppendLine("                    Ready to taste the difference? Order now and enjoy authentic kosher flavors delivered fresh.");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>💬 Share Feedback</strong><br>");
            contentBuilder.AppendLine("                    We value your opinion! Leave reviews and help us serve you better.");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("            </div>");
            contentBuilder.AppendLine("            <p>If you have any questions or need assistance, our team is here to help.</p>");
            contentBuilder.AppendLine("            <p>Thank you for choosing Kosher Clouds. We look forward to serving you!</p>");

            return GenerateEmailTemplate("Welcome to Kosher Clouds!", contentBuilder.ToString());
        }

        public string GenerateBookingConfirmationEmail(
            string userName,
            Guid bookingId,
            DateTime bookingDateTime)
        {
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"            <p>Dear {userName},</p>");
            contentBuilder.AppendLine("            <p>Your table reservation has been confirmed!</p>");
            contentBuilder.AppendLine("            <div class='order-details'>");
            contentBuilder.AppendLine($"                <p><strong>Booking ID:</strong> {bookingId}</p>");
            contentBuilder.AppendLine($"                <p><strong>Date & Time:</strong> {bookingDateTime:dddd, MMMM dd, yyyy 'at' HH:mm}</p>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>📍 Location</strong><br>");
            contentBuilder.AppendLine("                    Kosher Clouds Restaurant<br>");
            contentBuilder.AppendLine("                    We look forward to serving you!");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>⏰ Important Reminder</strong><br>");
            contentBuilder.AppendLine("                    Please arrive on time. If you need to cancel or modify your reservation, please contact us at least 2 hours in advance.");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("                <div class='item'>");
            contentBuilder.AppendLine("                    <strong>📞 Contact Us</strong><br>");
            contentBuilder.AppendLine("                    If you have any questions or special requests, feel free to reach out to us.");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("            </div>");
            contentBuilder.AppendLine("            <p>We're excited to welcome you to Kosher Clouds!</p>");
            contentBuilder.AppendLine("            <p>See you soon!</p>");

            return GenerateEmailTemplate("Booking Confirmation", contentBuilder.ToString());
        }

        public string GeneratePasswordResetEmail(
            string userName,
            string resetToken,
            string email,
            DateTime expiresAt)
        {
            var resetUrl = $"http://localhost:3000/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";
            var expiresIn = (expiresAt - DateTime.UtcNow).TotalMinutes;

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"            <p>Dear {userName},</p>");
            contentBuilder.AppendLine("            <p>We received a request to reset your password for your Kosher Clouds account.</p>");
            contentBuilder.AppendLine("            <div class='order-details'>");
            contentBuilder.AppendLine("                <p><strong>Password Reset Request</strong></p>");
            contentBuilder.AppendLine($"                <p>This link will expire in <strong>{expiresIn:F0} minutes</strong>.</p>");
            contentBuilder.AppendLine("                <div style='text-align: center; margin: 20px 0;'>");
            contentBuilder.AppendLine($"                    <a href='{resetUrl}' style='background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Reset Password</a>");
            contentBuilder.AppendLine("                </div>");
            contentBuilder.AppendLine("                <p style='color: #777; font-size: 14px;'>If the button doesn't work, copy and paste this link into your browser:</p>");
            contentBuilder.AppendLine($"                <p style='word-break: break-all; font-size: 12px; color: #555;'>{resetUrl}</p>");
            contentBuilder.AppendLine("            </div>");
            contentBuilder.AppendLine("            <p><strong>⚠️ Security Notice:</strong></p>");
            contentBuilder.AppendLine("            <ul style='color: #555;'>");
            contentBuilder.AppendLine("                <li>If you didn't request this password reset, please ignore this email.</li>");
            contentBuilder.AppendLine("                <li>Your password will remain unchanged.</li>");
            contentBuilder.AppendLine("                <li>Never share your password with anyone.</li>");
            contentBuilder.AppendLine("            </ul>");

            return GenerateEmailTemplate("Password Reset Request", contentBuilder.ToString());
        }
        private static string GenerateEmailTemplate(string title, string content)
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
            sb.AppendLine($"            <h1>{title}</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='content'>");
            sb.Append(content);
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