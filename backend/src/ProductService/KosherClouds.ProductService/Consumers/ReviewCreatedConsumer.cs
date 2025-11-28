using KosherClouds.Contracts.Reviews;
using KosherClouds.ProductService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ProductService.Consumers
{
    public class ReviewCreatedConsumer : IConsumer<ReviewCreatedEvent>
    {
        private readonly ProductDbContext _dbContext;
        private readonly ILogger<ReviewCreatedConsumer> _logger;

        public ReviewCreatedConsumer(
            ProductDbContext dbContext,
            ILogger<ReviewCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReviewCreatedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Received ReviewCreatedEvent for Product {ProductId}, Review {ReviewId}, Rating {Rating}",
                msg.ProductId, msg.ReviewId, msg.Rating);

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == msg.ProductId, context.CancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for review update", msg.ProductId);
                return;
            }

            var totalRating = (product.Rating * product.RatingCount) + msg.Rating;
            product.RatingCount += 1;
            product.Rating = Math.Round(totalRating / product.RatingCount, 2);
            product.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Product {ProductId} rating updated: {Rating} ({RatingCount} reviews)",
                msg.ProductId, product.Rating, product.RatingCount);
        }
    }
}