using KosherClouds.Contracts.Payments;
using KosherClouds.PaymentService.Data;
using KosherClouds.PaymentService.DTOs;
using KosherClouds.PaymentService.Services.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
            _logger.LogInformation("Creating Payment Intent for Order {OrderId}", request.OrderId);

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                ?? throw new InvalidOperationException("Stripe secret key not configured.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency,
                ReceiptEmail = request.ReceiptEmail,
                Description = $"Payment for Order {request.OrderId}",
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
                _logger.LogError(ex, "Failed to create Payment Intent for Order {OrderId}", request.OrderId);

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

            var payment = new DbPaymentRecord
            {
                OrderId = request.OrderId,
                UserId = userId,
                Amount = request.Amount,
                Provider = "Stripe",
                Status = "Pending",
                TransactionId = intent.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment Intent created: {PaymentId} ({StripeId})", payment.Id, intent.Id);

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                ClientSecret = intent.ClientSecret,
                Provider = payment.Provider,
                CreatedAt = payment.CreatedAt
            };
        }

        public async Task HandlePaymentSuccessAsync(
            string transactionId,
            CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for Transaction {TransactionId}", transactionId);
                return;
            }

            if (payment.Status == "Paid")
            {
                _logger.LogInformation("Payment {PaymentId} already marked as Paid", payment.Id);
                return;
            }

            payment.Status = "Paid";
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Payment {PaymentId} marked as Paid", payment.Id);

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
    }
}