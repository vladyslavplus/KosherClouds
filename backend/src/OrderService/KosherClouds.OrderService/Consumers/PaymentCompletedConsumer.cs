using KosherClouds.Contracts.Payments;
using KosherClouds.OrderService.Services.Interfaces;
using MassTransit;

namespace KosherClouds.OrderService.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        public PaymentCompletedConsumer(
            IOrderService orderService,
            ILogger<PaymentCompletedConsumer> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Received PaymentCompletedEvent for Order {OrderId}, PaymentId {PaymentId}",
                msg.OrderId, msg.PaymentId);

            await _orderService.MarkOrderAsPaidAsync(msg.OrderId, context.CancellationToken);

            _logger.LogInformation("Order {OrderId} marked as PAID", msg.OrderId);
        }
    }
}
