using KosherClouds.ReviewService.DTOs;
using KosherClouds.ReviewService.Parameters;
using KosherClouds.ReviewService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace KosherClouds.ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedList<ReviewResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReviews(
            [FromQuery] ReviewParameters parameters,
            CancellationToken cancellationToken)
        {
            if (!parameters.IsValidRatingRange)
                return BadRequest(new { message = "Invalid rating range" });

            if (!parameters.IsValidDateRange)
                return BadRequest(new { message = "Invalid date range" });

            var reviews = await _reviewService.GetReviewsAsync(parameters, cancellationToken);

            Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
            {
                reviews.TotalCount,
                reviews.PageSize,
                reviews.CurrentPage,
                reviews.TotalPages,
                reviews.HasNext,
                reviews.HasPrevious
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return Ok(reviews);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewById(Guid id, CancellationToken cancellationToken)
        {
            var review = await _reviewService.GetReviewByIdAsync(id, cancellationToken);

            if (review == null)
                return NotFound(new { message = "Review not found" });

            return Ok(review);
        }

        [HttpGet("my-orders-to-review")]
        [ProducesResponseType(typeof(List<OrderToReviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrdersToReview(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var orders = await _reviewService.GetOrdersToReviewAsync(userId.Value, cancellationToken);

            return Ok(orders);
        }

        [HttpGet("order/{orderId:guid}/reviewable-products")]
        [ProducesResponseType(typeof(List<ReviewableProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewableProducts(
            Guid orderId,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var products = await _reviewService.GetReviewableProductsAsync(
                orderId,
                userId.Value,
                cancellationToken);

            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReview(
            [FromBody] ReviewCreateDto dto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = await _reviewService.CreateReviewAsync(
                userId.Value,
                dto,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetReviewById),
                new { id = review.Id },
                review);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(
            Guid id,
            [FromBody] ReviewUpdateDto dto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = await _reviewService.UpdateReviewAsync(
                id,
                userId.Value,
                dto,
                cancellationToken);

            return Ok(review);
        }

        [HttpPost("{id:guid}/soft-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDeleteReview(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsAdmin();

            await _reviewService.SoftDeleteReviewAsync(id, userId.Value, isAdmin, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _reviewService.DeleteReviewAsync(id, cancellationToken);

            return NoContent();
        }

        [HttpPatch("{id:guid}/moderate")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ModerateReview(
            Guid id,
            [FromBody] ReviewModerationDto dto,
            CancellationToken cancellationToken)
        {
            var moderatorId = User.GetUserId();
            if (moderatorId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _reviewService.ModerateReviewAsync(id, moderatorId.Value, dto, cancellationToken);

            return NoContent();
        }
    }
}