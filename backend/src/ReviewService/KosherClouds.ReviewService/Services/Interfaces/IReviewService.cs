using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.ReviewService.Services.Interfaces
{
    public interface IReviewService
    {
        Task<PagedList<ReviewResponseDto>> GetReviewsAsync(
            ReviewParameters parameters,
            CancellationToken cancellationToken = default);

        Task<ReviewResponseDto?> GetReviewByIdAsync(
            Guid reviewId,
            CancellationToken cancellationToken = default);

        Task<List<OrderToReviewDto>> GetOrdersToReviewAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<ReviewableProductDto>> GetReviewableProductsAsync(
            Guid orderId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ReviewResponseDto> CreateReviewAsync(
            Guid userId,
            ReviewCreateDto dto,
            CancellationToken cancellationToken = default);

        Task<ReviewResponseDto> UpdateReviewAsync(
            Guid reviewId,
            Guid userId,
            ReviewUpdateDto dto,
            CancellationToken cancellationToken = default);

        Task SoftDeleteReviewAsync(
            Guid reviewId,
            Guid userId,
            bool isAdmin,
            CancellationToken cancellationToken = default);

        Task DeleteReviewAsync(
            Guid reviewId,
            CancellationToken cancellationToken = default);

        Task ModerateReviewAsync(
            Guid reviewId,
            Guid moderatorId,
            ReviewModerationDto dto,
            CancellationToken cancellationToken = default);
    }
}