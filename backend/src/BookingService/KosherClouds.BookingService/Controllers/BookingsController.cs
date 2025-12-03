using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Parameters;
using KosherClouds.BookingService.Services.Interfaces;
using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.ServiceDefaults.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace KosherClouds.BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedList<BookingResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookings(
            [FromQuery] BookingParameters parameters,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdminOrManager = User.IsAdminOrManager();

            if (!isAdminOrManager)
            {
                parameters.UserId = userId;
            }

            var bookings = await _bookingService.GetBookingsAsync(
                parameters,
                isAdminOrManager,
                cancellationToken);

            Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
            {
                bookings.TotalCount,
                bookings.PageSize,
                bookings.CurrentPage,
                bookings.TotalPages,
                bookings.HasNext,
                bookings.HasPrevious
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return Ok(bookings);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingById(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdminOrManager = User.IsAdminOrManager();

            var booking = await _bookingService.GetBookingByIdAsync(
                id,
                userId.Value,
                isAdminOrManager,
                cancellationToken);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(booking);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBooking(
            [FromBody] BookingCreateDto dto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var booking = await _bookingService.CreateBookingAsync(
                userId.Value,
                dto,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetBookingById),
                new { id = booking.Id },
                booking);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBooking(
            Guid id,
            [FromBody] BookingUpdateDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _bookingService.UpdateBookingAsync(id, dto, cancellationToken);

            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBooking(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdminOrManager = User.IsAdminOrManager();

            await _bookingService.CancelBookingAsync(
                id,
                userId.Value,
                isAdminOrManager,
                cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsAdmin();

            await _bookingService.DeleteBookingAsync(
                id,
                userId.Value,
                isAdmin,
                cancellationToken);

            return NoContent();
        }
    }
}