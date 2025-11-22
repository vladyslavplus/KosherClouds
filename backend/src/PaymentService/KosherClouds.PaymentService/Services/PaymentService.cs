using KosherClouds.Contracts.Payments;
using KosherClouds.PaymentService.Data;
using KosherClouds.PaymentService.DTOs;
using KosherClouds.PaymentService.Services.Interfaces;
using MassTransit;
using Stripe;
using DbPaymentRecord = KosherClouds.PaymentService.Entities.PaymentRecord;

namespace KosherClouds.PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<PaymentService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentService(
            PaymentDbContext dbContext,
            ILogger<PaymentService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(
            PaymentRequestDto request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting payment for Order {OrderId}", request.OrderId);

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                ?? throw new InvalidOperationException("Stripe secret key not configured.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency,
                ReceiptEmail = request.ReceiptEmail,
                Description = $"Payment for Order {request.OrderId}",
                // TODO: with frontend - change
                PaymentMethod = "pm_card_visa", 
                Confirm = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() },
                    { "user_id", userId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            PaymentIntent intent;

            try
            {
                intent = await service.CreateAsync(options, cancellationToken: cancellationToken);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe payment failed for Order {OrderId}", request.OrderId);

                var failed = new DbPaymentRecord
                {
                    OrderId = request.OrderId,
                    UserId = userId,
                    Amount = request.Amount,
                    Provider = "Stripe",
                    Status = "Failed",
                    FailureReason = ex.Message
                };

                _dbContext.Payments.Add(failed);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new PaymentResponseDto
                {
                    PaymentId = failed.Id,
                    Status = failed.Status,
                    Provider = failed.Provider,
                    CreatedAt = failed.CreatedAt
                };
            }

            var paymentStatus = intent.Status == "succeeded" ? "Paid" : "Pending";

            var payment = new DbPaymentRecord
            {
                OrderId = request.OrderId,
                UserId = userId,
                Amount = request.Amount,
                Provider = "Stripe",
                Status = paymentStatus,
                TransactionId = intent.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Payment {Status}: {PaymentId} ({StripeId})", paymentStatus, payment.Id, intent.Id);

            if (paymentStatus == "Paid")
            {
                await _publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    TransactionId = payment.TransactionId!,
                    CompletedAt = payment.UpdatedAt
                }, cancellationToken);

                _logger.LogInformation("Published PaymentCompletedEvent for Payment {PaymentId}", payment.Id);
            }

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                Provider = payment.Provider,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}