using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;

namespace KosherClouds.BookingService.Services.Interfaces
{
    public interface IBookingService
    {
        Task<PagedList<BookingResponseDto>> GetBookingsAsync(
            BookingParameters parameters,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default);

        Task<BookingResponseDto?> GetBookingByIdAsync(
            Guid id,
            Guid userId,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default);

        Task<BookingResponseDto> CreateBookingAsync(
            Guid userId,
            BookingCreateDto dto,
            CancellationToken cancellationToken = default);

        Task UpdateBookingAsync(
            Guid id,
            BookingUpdateDto dto,
            CancellationToken cancellationToken = default);

        Task CancelBookingAsync(
            Guid id,
            Guid userId,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default);

        Task DeleteBookingAsync(
            Guid id,
            Guid userId,
            bool isAdmin = false,
            CancellationToken cancellationToken = default);
    }
}