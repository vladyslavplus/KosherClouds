using KosherClouds.Contracts.Orders;
using KosherClouds.Contracts.Payments;
using KosherClouds.OrderService.Data;
using KosherClouds.OrderService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        public PaymentCompletedConsumer(
            OrderDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentCompletedConsumer> logger)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Received PaymentCompletedEvent for Order {OrderId}, PaymentId {PaymentId}",
                msg.OrderId, msg.PaymentId);

            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == msg.OrderId, context.CancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", msg.OrderId);
                return;
            }

            order.Status = OrderStatus.Paid;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Order {OrderId} marked as PAID", msg.OrderId);

            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemInfo
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductNameSnapshot,
                    UnitPrice = item.UnitPriceSnapshot,
                    Quantity = item.Quantity,
                    LineTotal = item.LineTotal
                }).ToList()
            }, context.CancellationToken);

            _logger.LogInformation(
                "OrderCreatedEvent published for Order {OrderId} after payment completion",
                msg.OrderId);
        }
    }
}