using KosherClouds.Contracts.Bookings;
using KosherClouds.NotificationService.Models;
using KosherClouds.NotificationService.Services.External;
using KosherClouds.NotificationService.Services.Interfaces;
using MassTransit;

namespace KosherClouds.NotificationService.Consumers
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IUserApiClient _userApiClient;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<BookingCreatedConsumer> _logger;

        public BookingCreatedConsumer(
            IEmailService emailService,
            IUserApiClient userApiClient,
            IEmailTemplateService emailTemplateService,
            ILogger<BookingCreatedConsumer> logger)
        {
            _emailService = emailService;
            _userApiClient = userApiClient;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var bookingEvent = context.Message;

            _logger.LogInformation(
                "Processing BookingCreatedEvent for Booking {BookingId}, User {UserId}",
                bookingEvent.BookingId,
                bookingEvent.UserId);

            try
            {
                var user = await _userApiClient.GetUserByIdAsync(
                    bookingEvent.UserId,
                    context.CancellationToken);

                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning(
                        "User {UserId} not found or has no email. Skipping notification.",
                        bookingEvent.UserId);
                    return;
                }

                var userName = !string.IsNullOrWhiteSpace(user.FirstName)
                    ? user.FirstName
                    : "Valued Customer";

                var emailBody = _emailTemplateService.GenerateBookingConfirmationEmail(
                    userName,
                    bookingEvent.BookingId,
                    bookingEvent.BookingDateTime);

                var emailMessage = new EmailMessage
                {
                    To = user.Email,
                    Subject = $"Booking Confirmation #{bookingEvent.BookingId.ToString()[..8]}",
                    Body = emailBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailMessage, context.CancellationToken);

                _logger.LogInformation(
                    "Booking confirmation email sent to {Email} for Booking {BookingId}",
                    user.Email,
                    bookingEvent.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send booking confirmation email for Booking {BookingId}",
                    bookingEvent.BookingId);
            }
        }
    }
}