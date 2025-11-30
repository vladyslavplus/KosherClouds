using AutoMapper;
using KosherClouds.Contracts.Reviews;
using KosherClouds.ReviewService.Data;
using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.DTOs.External;
using KosherClouds.ReviewService.Entities;
using KosherClouds.ReviewService.Parameters;
using KosherClouds.ReviewService.Services.External;
using KosherClouds.ReviewService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ReviewService.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ISortHelperFactory _sortFactory;
        private readonly ILogger<ReviewService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IUserApiClient _userApiClient;

        private const int ReviewPeriodDays = 14;

        public ReviewService(
            ReviewDbContext dbContext,
            IMapper mapper,
            ISortHelperFactory sortFactory,
            ILogger<ReviewService> logger,
            IPublishEndpoint publishEndpoint,
            IOrderApiClient orderApiClient,
            IUserApiClient userApiClient)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _sortFactory = sortFactory;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _orderApiClient = orderApiClient;
            _userApiClient = userApiClient;
        }

        public async Task<PagedList<ReviewResponseDto>> GetReviewsAsync(
            ReviewParameters parameters,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Review> query = _dbContext.Reviews.AsNoTracking();

            if (parameters.ProductId.HasValue)
                query = query.Where(r => r.ProductId == parameters.ProductId.Value);

            if (parameters.UserId.HasValue)
                query = query.Where(r => r.UserId == parameters.UserId.Value);

            if (parameters.OrderId.HasValue)
                query = query.Where(r => r.OrderId == parameters.OrderId.Value);


            if (!string.IsNullOrWhiteSpace(parameters.ReviewType) &&
                Enum.TryParse<ReviewType>(parameters.ReviewType, true, out var reviewTypeEnum))
            {
                query = query.Where(r => r.ReviewType == reviewTypeEnum);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Status) &&
                Enum.TryParse<ReviewStatus>(parameters.Status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }
            else
            {
                query = query.Where(r => r.Status == ReviewStatus.Published);
            }

            if (parameters.MinRating.HasValue)
                query = query.Where(r => r.Rating >= parameters.MinRating.Value);

            if (parameters.MaxRating.HasValue)
                query = query.Where(r => r.Rating <= parameters.MaxRating.Value);

            if (parameters.MinCreatedDate.HasValue)
                query = query.Where(r => r.CreatedAt >= parameters.MinCreatedDate.Value);

            if (parameters.MaxCreatedDate.HasValue)
                query = query.Where(r => r.CreatedAt <= parameters.MaxCreatedDate.Value);

            if (parameters.VerifiedOnly == true)
                query = query.Where(r => r.IsVerifiedPurchase);

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(r =>
                    r.Comment != null &&
                    EF.Functions.ILike(r.Comment, $"%{parameters.SearchTerm}%"));
            }

            var sortHelper = _sortFactory.Create<Review>();
            query = sortHelper.ApplySort(query, parameters.OrderBy);

            var pagedReviews = await PagedList<Review>.ToPagedListAsync(
                query,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken);

            var reviewDtos = _mapper.Map<List<ReviewResponseDto>>(pagedReviews);

            await EnrichReviewsWithUserNamesAsync(reviewDtos, cancellationToken);
            await EnrichReviewsWithProductNamesAsync(reviewDtos, cancellationToken);

            return new PagedList<ReviewResponseDto>(
                reviewDtos,
                pagedReviews.TotalCount,
                pagedReviews.CurrentPage,
                pagedReviews.PageSize);
        }

        public async Task<ReviewResponseDto?> GetReviewByIdAsync(
            Guid reviewId,
            CancellationToken cancellationToken = default)
        {
            var review = await _dbContext.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

            if (review == null)
                return null;

            var reviewDto = _mapper.Map<ReviewResponseDto>(review);

            await EnrichReviewsWithUserNamesAsync(new List<ReviewResponseDto> { reviewDto }, cancellationToken);
            await EnrichReviewsWithProductNamesAsync(new List<ReviewResponseDto> { reviewDto }, cancellationToken);

            return reviewDto;
        }

        public async Task<List<OrderToReviewDto>> GetOrdersToReviewAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting orders to review for user {UserId}", userId);

            var orders = await _orderApiClient.GetPaidOrdersForUserAsync(userId, ReviewPeriodDays, cancellationToken);

            if (!orders.Any())
            {
                _logger.LogInformation("No paid orders found for user {UserId}", userId);
                return new List<OrderToReviewDto>();
            }

            var orderIds = orders.Select(o => o.Id).ToList();
            var existingReviews = await _dbContext.Reviews
                .Where(r => orderIds.Contains(r.OrderId) && r.UserId == userId)
                .ToListAsync(cancellationToken);

            var result = new List<OrderToReviewDto>();

            foreach (var order in orders)
            {
                var orderReviews = existingReviews.Where(r => r.OrderId == order.Id).ToList();
                var orderReview = orderReviews.FirstOrDefault(r => r.ReviewType == ReviewType.Order);

                var reviewableProducts = order.Items.Select(item => new ReviewableProductDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductNameSnapshot,
                    ProductNameUk = item.ProductNameSnapshotUk,
                    Price = item.UnitPriceSnapshot,
                    Quantity = item.Quantity,
                    AlreadyReviewed = orderReviews.Any(r => r.ReviewType == ReviewType.Product && r.ProductId == item.ProductId),
                    ExistingReviewId = orderReviews.FirstOrDefault(r => r.ReviewType == ReviewType.Product && r.ProductId == item.ProductId)?.Id
                }).ToList();

                var daysLeft = ReviewPeriodDays - (int)(DateTimeOffset.UtcNow - order.CreatedAt).TotalDays;

                result.Add(new OrderToReviewDto
                {
                    OrderId = order.Id,
                    OrderDate = order.CreatedAt,
                    TotalAmount = order.TotalAmount,
                    OrderReviewExists = orderReview != null,
                    OrderReviewId = orderReview?.Id,
                    Products = reviewableProducts,
                    ReviewableProductsCount = reviewableProducts.Count(p => !p.AlreadyReviewed),
                    DaysLeftToReview = Math.Max(0, daysLeft)
                });
            }

            _logger.LogInformation("Found {Count} orders to review for user {UserId}", result.Count, userId);

            return result;
        }

        public async Task<List<ReviewableProductDto>> GetReviewableProductsAsync(
            Guid orderId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting reviewable products for order {OrderId} and user {UserId}", orderId, userId);

            var order = await _orderApiClient.GetOrderByIdAsync(orderId, cancellationToken);

            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("You can only view your own orders");

            if (order.Status != "Paid")
                throw new InvalidOperationException("You can only review paid orders");

            var reviewDeadline = order.CreatedAt.AddDays(ReviewPeriodDays);
            if (DateTimeOffset.UtcNow > reviewDeadline)
                throw new InvalidOperationException($"Review period has expired ({ReviewPeriodDays} days)");

            var existingReviews = await _dbContext.Reviews
                .Where(r => r.OrderId == orderId && r.UserId == userId && r.ReviewType == ReviewType.Product)
                .Select(r => new { r.ProductId, r.Id })
                .ToListAsync(cancellationToken);

            var result = order.Items.Select(item => new ReviewableProductDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductNameSnapshot,
                ProductNameUk = item.ProductNameSnapshotUk,
                Price = item.UnitPriceSnapshot,
                Quantity = item.Quantity,
                AlreadyReviewed = existingReviews.Any(r => r.ProductId == item.ProductId),
                ExistingReviewId = existingReviews.FirstOrDefault(r => r.ProductId == item.ProductId)?.Id
            }).ToList();

            _logger.LogInformation("Found {Count} reviewable products for order {OrderId}", result.Count, orderId);

            return result;
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(
            Guid userId,
            ReviewCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating {ReviewType} review for Order {OrderId} by User {UserId}", dto.ReviewType, dto.OrderId, userId);

            var order = await _orderApiClient.GetOrderByIdAsync(dto.OrderId, cancellationToken);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("This order doesn't belong to you");

            if (order.Status != "Paid")
                throw new InvalidOperationException("You can only review paid orders");

            var reviewDeadline = order.CreatedAt.AddDays(ReviewPeriodDays);
            if (DateTimeOffset.UtcNow > reviewDeadline)
                throw new InvalidOperationException($"Review period has expired ({ReviewPeriodDays} days)");

            if (dto.ReviewType == ReviewType.Order)
            {
                if (dto.ProductId.HasValue)
                    throw new InvalidOperationException("Order reviews cannot have ProductId");

                var existingOrderReview = await _dbContext.Reviews
                    .AnyAsync(r => r.OrderId == dto.OrderId && r.UserId == userId && r.ReviewType == ReviewType.Order, cancellationToken);

                if (existingOrderReview)
                    throw new InvalidOperationException("You already reviewed this order");
            }
            else if (dto.ReviewType == ReviewType.Product)
            {
                if (!dto.ProductId.HasValue)
                    throw new InvalidOperationException("ProductId is required for product reviews");

                if (!order.Items.Any(i => i.ProductId == dto.ProductId.Value))
                    throw new InvalidOperationException("This product was not in your order");

                var existingProductReview = await _dbContext.Reviews
                    .AnyAsync(r => r.OrderId == dto.OrderId && r.ProductId == dto.ProductId.Value && r.UserId == userId && r.ReviewType == ReviewType.Product, cancellationToken);

                if (existingProductReview)
                    throw new InvalidOperationException("You already reviewed this product in this order");
            }

            var review = _mapper.Map<Review>(dto);
            review.UserId = userId;
            review.IsVerifiedPurchase = true;
            review.Status = ReviewStatus.Published;

            await _dbContext.Reviews.AddAsync(review, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} created successfully", review.Id);

            if (review.ReviewType == ReviewType.Product && review.ProductId.HasValue)
            {
                await _publishEndpoint.Publish(new ReviewCreatedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId.Value,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    CreatedAt = review.CreatedAt
                }, cancellationToken);
            }

            var result = _mapper.Map<ReviewResponseDto>(review);
            await EnrichReviewsWithUserNamesAsync(new List<ReviewResponseDto> { result }, cancellationToken);
            await EnrichReviewsWithProductNamesAsync(new List<ReviewResponseDto> { result }, cancellationToken);

            return result;
        }

        public async Task<ReviewResponseDto> UpdateReviewAsync(
            Guid reviewId,
            Guid userId,
            ReviewUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating review {ReviewId} by user {UserId}", reviewId, userId);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

            if (review == null)
                throw new KeyNotFoundException("Review not found");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own reviews");

            if (review.Status == ReviewStatus.Deleted)
                throw new InvalidOperationException("Cannot update deleted review");

            if (review.Status == ReviewStatus.Hidden)
                throw new InvalidOperationException("Cannot update hidden review. Contact support.");

            var updateDeadline = review.CreatedAt.AddDays(ReviewPeriodDays);
            if (DateTimeOffset.UtcNow > updateDeadline)
                throw new InvalidOperationException($"Review can only be updated within {ReviewPeriodDays} days after creation");

            var oldRating = review.Rating;

            if (dto.Rating.HasValue)
                review.Rating = dto.Rating.Value;

            if (dto.Comment != null)
                review.Comment = dto.Comment;

            review.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} updated successfully", reviewId);

            if (review.ReviewType == ReviewType.Product && review.ProductId.HasValue && oldRating != review.Rating)
            {
                await _publishEndpoint.Publish(new ReviewUpdatedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId.Value,
                    OldRating = oldRating,
                    NewRating = review.Rating,
                    UpdatedAt = review.UpdatedAt.Value
                }, cancellationToken);
            }

            var result = _mapper.Map<ReviewResponseDto>(review);
            await EnrichReviewsWithUserNamesAsync(new List<ReviewResponseDto> { result }, cancellationToken);
            await EnrichReviewsWithProductNamesAsync(new List<ReviewResponseDto> { result }, cancellationToken);

            return result;
        }

        public async Task SoftDeleteReviewAsync(
            Guid reviewId,
            Guid userId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Soft deleting review {ReviewId} by user {UserId} (isAdmin: {IsAdmin})", reviewId, userId, isAdmin);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

            if (review == null)
                throw new KeyNotFoundException("Review not found");

            if (review.UserId != userId && !isAdmin)
                throw new UnauthorizedAccessException("You can only delete your own reviews");

            if (review.Status == ReviewStatus.Deleted)
                throw new InvalidOperationException("Review is already deleted");

            var oldStatus = review.Status.ToString();
            review.Status = ReviewStatus.Deleted;
            review.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} soft deleted", reviewId);

            if (review.ReviewType == ReviewType.Product && review.ProductId.HasValue)
            {
                await _publishEndpoint.Publish(new ReviewDeletedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId.Value,
                    Rating = review.Rating,
                    DeletedAt = review.UpdatedAt.Value
                }, cancellationToken);

                await _publishEndpoint.Publish(new ReviewStatusChangedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId.Value,
                    Rating = review.Rating,
                    OldStatus = oldStatus,
                    NewStatus = ReviewStatus.Deleted.ToString(),
                    ChangedAt = review.UpdatedAt.Value
                }, cancellationToken);
            }
        }

        public async Task DeleteReviewAsync(
            Guid reviewId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Hard deleting review {ReviewId} by Admin", reviewId);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

            if (review == null)
                throw new KeyNotFoundException("Review not found");

            var productId = review.ProductId;
            var rating = review.Rating;
            var reviewType = review.ReviewType;

            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} permanently deleted", reviewId);

            if (reviewType == ReviewType.Product && productId.HasValue)
            {
                await _publishEndpoint.Publish(new ReviewDeletedEvent
                {
                    ReviewId = reviewId,
                    ProductId = productId.Value,
                    Rating = rating,
                    DeletedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
        }

        public async Task ModerateReviewAsync(
            Guid reviewId,
            Guid moderatorId,
            ReviewModerationDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Moderating review {ReviewId} by moderator {ModeratorId}. Action: {Action}", reviewId, moderatorId, dto.Action);

            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

            if (review == null)
                throw new KeyNotFoundException("Review not found");

            if (review.Status == ReviewStatus.Deleted)
                throw new InvalidOperationException("Cannot moderate deleted review");

            var oldStatus = review.Status.ToString();

            switch (dto.Action.ToLower())
            {
                case "hide":
                    if (review.Status == ReviewStatus.Hidden)
                        throw new InvalidOperationException("Review is already hidden");

                    review.Status = ReviewStatus.Hidden;
                    review.ModerationNotes = dto.ModerationNotes;
                    review.ModeratedBy = moderatorId;
                    review.ModeratedAt = DateTimeOffset.UtcNow;
                    review.UpdatedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Review {ReviewId} hidden by moderator {ModeratorId}", reviewId, moderatorId);
                    break;

                case "publish":
                    if (review.Status == ReviewStatus.Published)
                        throw new InvalidOperationException("Review is already published");

                    review.Status = ReviewStatus.Published;
                    review.ModerationNotes = dto.ModerationNotes;
                    review.ModeratedBy = moderatorId;
                    review.ModeratedAt = DateTimeOffset.UtcNow;
                    review.UpdatedAt = DateTimeOffset.UtcNow;

                    _logger.LogInformation("Review {ReviewId} published by moderator {ModeratorId}", reviewId, moderatorId);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid moderation action: {dto.Action}. Use 'Hide' or 'Publish'");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (review.ReviewType == ReviewType.Product && review.ProductId.HasValue)
            {
                await _publishEndpoint.Publish(new ReviewStatusChangedEvent
                {
                    ReviewId = review.Id,
                    ProductId = review.ProductId.Value,
                    Rating = review.Rating,
                    OldStatus = oldStatus,
                    NewStatus = review.Status.ToString(),
                    ChangedAt = review.UpdatedAt.Value
                }, cancellationToken);
            }

            _logger.LogInformation("Review {ReviewId} moderation completed. Status changed from {OldStatus} to {NewStatus}", reviewId, oldStatus, review.Status);
        }

        private async Task EnrichReviewsWithUserNamesAsync(
            List<ReviewResponseDto> reviews,
            CancellationToken cancellationToken)
        {
            if (!reviews.Any())
                return;

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var userNames = await _userApiClient.GetUserNamesByIdsAsync(userIds, cancellationToken);

            foreach (var review in reviews)
            {
                if (userNames.TryGetValue(review.UserId, out var userName))
                {
                    review.UserName = userName;
                }
            }
        }

        private async Task EnrichReviewsWithProductNamesAsync(
            List<ReviewResponseDto> reviews,
            CancellationToken cancellationToken)
        {
            if (!reviews.Any())
                return;

            var productReviews = reviews.Where(r => r.ProductId.HasValue).ToList();
            if (!productReviews.Any())
                return;

            var orderIds = productReviews.Select(r => r.OrderId).Distinct().ToList();
            var ordersDict = new Dictionary<Guid, OrderDto>();

            foreach (var orderId in orderIds)
            {
                var order = await _orderApiClient.GetOrderByIdAsync(orderId, cancellationToken);
                if (order != null)
                {
                    ordersDict[orderId] = order;
                }
            }

            foreach (var review in productReviews)
            {
                if (ordersDict.TryGetValue(review.OrderId, out var order))
                {
                    var orderItem = order.Items.FirstOrDefault(i => i.ProductId == review.ProductId!.Value);
                    if (orderItem != null)
                    {
                        review.ProductName = orderItem.ProductNameSnapshot;
                        review.ProductNameUk = orderItem.ProductNameSnapshotUk;
                    }
                }
            }
        }
    }
}