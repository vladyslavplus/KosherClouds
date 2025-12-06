using AutoMapper;
using KosherClouds.BookingService.Data;
using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Entities;
using KosherClouds.BookingService.Parameters;
using KosherClouds.BookingService.Services.Interfaces;
using KosherClouds.Contracts.Bookings;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ISortHelperFactory _sortFactory;
        private readonly IPublishEndpoint _publishEndpoint;

        private const int OpeningHour = 10;
        private const int ClosingHour = 23;
        private const int BookingBufferMinutes = 30;
        private const int MaxGuestsPerBooking = 50;
        private const int MaxAdvanceBookingDays = 60;

        public BookingService(
            BookingDbContext dbContext,
            IMapper mapper,
            ISortHelperFactory sortFactory,
            IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _sortFactory = sortFactory;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<PagedList<BookingResponseDto>> GetBookingsAsync(
            BookingParameters parameters,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Booking> query = _dbContext.Bookings
                .Include(b => b.Hookahs)
                .AsNoTracking();

            if (!isAdminOrManager)
            {
                if (!parameters.UserId.HasValue)
                    throw new UnauthorizedAccessException("UserId is required.");

                query = query.Where(b => b.UserId == parameters.UserId.Value);
            }
            else
            {
                if (parameters.UserId.HasValue)
                    query = query.Where(b => b.UserId == parameters.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                query = query.Where(b => b.PhoneNumber.Contains(parameters.SearchTerm)
                                           || (b.Comment != null && b.Comment.Contains(parameters.SearchTerm)));

            if (parameters.MinBookingDate.HasValue)
                query = query.Where(b => b.BookingDateTime >= parameters.MinBookingDate.Value);

            if (parameters.MaxBookingDate.HasValue)
                query = query.Where(b => b.BookingDateTime <= parameters.MaxBookingDate.Value);

            var sortHelper = _sortFactory.Create<Booking>();
            query = sortHelper.ApplySort(query, parameters.OrderBy);

            var pagedBookings = await PagedList<Booking>.ToPagedListAsync(
                query,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken);

            var dtoList = _mapper.Map<IEnumerable<BookingResponseDto>>(pagedBookings);

            return new PagedList<BookingResponseDto>(
                dtoList.ToList(),
                pagedBookings.TotalCount,
                pagedBookings.CurrentPage,
                pagedBookings.PageSize);
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(
            Guid id,
            Guid userId,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Bookings
                .Include(b => b.Hookahs)
                .AsNoTracking()
                .Where(b => b.Id == id);

            if (!isAdminOrManager)
            {
                query = query.Where(b => b.UserId == userId);
            }

            var booking = await query.FirstOrDefaultAsync(cancellationToken);
            return _mapper.Map<BookingResponseDto?>(booking);
        }

        public async Task<BookingResponseDto> CreateBookingAsync(Guid userId, BookingCreateDto dto, CancellationToken cancellationToken = default)
        {
            var bookingDateTime = DateTime.SpecifyKind(dto.BookingDateTime, DateTimeKind.Utc);

            await ValidateBookingBusinessRulesAsync(dto, cancellationToken);

            var booking = _mapper.Map<Booking>(dto);
            booking.UserId = userId;
            booking.Status = BookingStatus.Pending;
            booking.CreatedAt = DateTime.UtcNow;
            booking.BookingDateTime = bookingDateTime;

            await _dbContext.Bookings.AddAsync(booking, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                BookingDateTime = booking.BookingDateTime,
                CreatedAt = booking.CreatedAt
            }, cancellationToken);

            return _mapper.Map<BookingResponseDto>(booking);
        }

        public async Task UpdateBookingAsync(
            Guid id,
            BookingUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Hookahs)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Cannot modify a cancelled booking");

            if (booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Cannot modify a completed booking");

            if (dto.BookingDateTime.HasValue || !string.IsNullOrWhiteSpace(dto.Zone))
            {
                var tempDto = new BookingCreateDto
                {
                    BookingDateTime = dto.BookingDateTime ?? booking.BookingDateTime,
                    Zone = dto.Zone ?? booking.Zone.ToString(),
                    Adults = dto.Adults ?? booking.Adults,
                    Children = dto.Children ?? booking.Children
                };

                await ValidateBookingBusinessRulesAsync(tempDto, cancellationToken, booking.Id);
            }

            if (dto.BookingDateTime.HasValue)
                booking.BookingDateTime = DateTime.SpecifyKind(dto.BookingDateTime.Value, DateTimeKind.Utc);

            if (dto.Adults.HasValue)
                booking.Adults = dto.Adults.Value;

            if (dto.Children.HasValue)
                booking.Children = dto.Children.Value;

            if (!string.IsNullOrWhiteSpace(dto.Zone))
                booking.Zone = Enum.Parse<BookingZone>(dto.Zone, true);

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                booking.PhoneNumber = dto.PhoneNumber;

            if (dto.Comment != null)
                booking.Comment = dto.Comment;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                booking.Status = Enum.Parse<BookingStatus>(dto.Status, true);

            if (dto.Hookahs != null)
            {
                booking.Hookahs.Clear();
                foreach (var hookahDto in dto.Hookahs)
                {
                    booking.Hookahs.Add(new HookahBooking
                    {
                        ProductId = hookahDto.ProductId,
                        ProductName = hookahDto.ProductName ?? "Unknown",
                        ProductNameUk = hookahDto.ProductNameUk,
                        TobaccoFlavor = hookahDto.TobaccoFlavor,
                        TobaccoFlavorUk = hookahDto.TobaccoFlavorUk,
                        Strength = Enum.Parse<HookahStrength>(hookahDto.Strength, true),
                        ServeAfterMinutes = hookahDto.ServeAfterMinutes,
                        Notes = hookahDto.Notes,
                        PriceSnapshot = hookahDto.PriceSnapshot ?? 0m
                    });
                }
            }

            booking.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new BookingUpdatedEvent
            {
                BookingId = booking.Id,
                Comment = booking.Comment,
                UpdatedAt = booking.UpdatedAt.Value
            }, cancellationToken);
        }

        public async Task CancelBookingAsync(
            Guid id,
            Guid userId,
            bool isAdminOrManager = false,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Bookings.Where(b => b.Id == id);

            if (!isAdminOrManager)
            {
                query = query.Where(b => b.UserId == userId);
            }

            var booking = await query.FirstOrDefaultAsync(cancellationToken);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled");

            if (booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed booking");

            if (booking.Status == BookingStatus.NoShow)
                throw new InvalidOperationException("Cannot cancel a no-show booking");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new BookingCancelledEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                CancelledAt = DateTime.UtcNow,
                OriginalBookingDateTime = booking.BookingDateTime
            }, cancellationToken);
        }

        public async Task DeleteBookingAsync(
            Guid id,
            Guid userId,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Bookings.Where(b => b.Id == id);

            if (!isAdmin)
            {
                query = query.Where(b => b.UserId == userId);
            }

            var booking = await query.FirstOrDefaultAsync(cancellationToken);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (booking.Status == BookingStatus.Pending && booking.BookingDateTime > DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Cannot delete an active booking. Please cancel it first.");
            }

            _dbContext.Bookings.Remove(booking);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new BookingDeletedEvent
            {
                BookingId = booking.Id,
                UserId = booking.UserId,
                DeletedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        #region Business Validation

        private async Task ValidateBookingBusinessRulesAsync(
            BookingCreateDto dto,
            CancellationToken cancellationToken,
            Guid? excludeBookingId = null)
        {
            ValidateWorkingHours(dto.BookingDateTime);

            ValidateAdvanceBooking(dto.BookingDateTime);

            ValidateGuestCount(dto.Adults, dto.Children);

            await ValidateNoBookingConflictAsync(
                dto.BookingDateTime,
                dto.Zone,
                cancellationToken,
                excludeBookingId);

            if (dto.Hookahs != null && dto.Hookahs.Any())
            {
                ValidateHookahs(dto.Hookahs);
            }
        }

        private static void ValidateWorkingHours(DateTime bookingDateTime)
        {
            var hour = bookingDateTime.Hour;
            if (hour < OpeningHour || hour >= ClosingHour)
            {
                throw new InvalidOperationException(
                    $"Bookings are only available between {OpeningHour}:00 and {ClosingHour}:00");
            }
        }

        private static void ValidateAdvanceBooking(DateTime bookingDateTime)
        {
            var maxDate = DateTime.UtcNow.AddDays(MaxAdvanceBookingDays);
            if (bookingDateTime > maxDate)
            {
                throw new InvalidOperationException(
                    $"Bookings can only be made up to {MaxAdvanceBookingDays} days in advance");
            }
        }

        private static void ValidateGuestCount(int adults, int children)
        {
            var totalGuests = adults + children;

            if (totalGuests < 1)
            {
                throw new InvalidOperationException("At least one guest is required");
            }

            if (totalGuests > MaxGuestsPerBooking)
            {
                throw new InvalidOperationException(
                    $"Total number of guests cannot exceed {MaxGuestsPerBooking}");
            }
        }

        private static void ValidateHookahs(List<HookahBookingDto> hookahs)
        {
            if (hookahs.Count > 5)
                throw new InvalidOperationException("Cannot add more than 5 hookahs per booking");

            var invalidHookah = hookahs.FirstOrDefault(h => h.ServeAfterMinutes.HasValue && h.ServeAfterMinutes.Value > 240);
            if (invalidHookah != null)
                throw new InvalidOperationException("Hookah serve time cannot exceed 240 minutes (4 hours)");
        }

        private async Task ValidateNoBookingConflictAsync(
            DateTime bookingDateTime,
            string zone,
            CancellationToken cancellationToken,
            Guid? excludeBookingId = null)
        {
            var hasConflict = await CheckBookingConflictAsync(
                bookingDateTime,
                zone,
                cancellationToken,
                excludeBookingId);

            if (hasConflict)
            {
                throw new InvalidOperationException(
                    "This time slot is already booked for the selected zone. " +
                    $"Please choose a different time (at least {BookingBufferMinutes} minutes apart)");
            }
        }

        private async Task<bool> CheckBookingConflictAsync(
            DateTime bookingDateTime,
            string zone,
            CancellationToken cancellationToken,
            Guid? excludeBookingId = null)
        {
            var startTime = DateTime.SpecifyKind(bookingDateTime.AddMinutes(-BookingBufferMinutes), DateTimeKind.Utc);
            var endTime = DateTime.SpecifyKind(bookingDateTime.AddMinutes(BookingBufferMinutes), DateTimeKind.Utc);
            var zoneEnum = Enum.Parse<BookingZone>(zone, true);

            var query = _dbContext.Bookings
                .Where(b => b.Zone == zoneEnum
                         && b.Status != BookingStatus.Cancelled
                         && b.BookingDateTime >= startTime
                         && b.BookingDateTime <= endTime);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        #endregion
    }
}