using AutoMapper;
using FluentAssertions;
using KosherClouds.BookingService.Data;
using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Entities;
using KosherClouds.BookingService.Parameters;
using KosherClouds.BookingService.UnitTests.Helpers;
using KosherClouds.Contracts.Bookings;
using KosherClouds.ServiceDefaults.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using BookingServiceClass = KosherClouds.BookingService.Services.BookingService;

namespace KosherClouds.BookingService.UnitTests.Services
{
    public class BookingServiceTests : IDisposable
    {
        private readonly BookingDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<ISortHelperFactory> _sortHelperFactoryMock;
        private readonly Mock<ISortHelper<Booking>> _sortHelperMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly BookingServiceClass _bookingService;
        private bool _disposed;

        public BookingServiceTests()
        {
            _dbContext = MockBookingDbContextFactory.Create();
            _mapper = AutoMapperFactory.Create();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            _sortHelperMock = new Mock<ISortHelper<Booking>>();
            _sortHelperMock
                .Setup(x => x.ApplySort(It.IsAny<IQueryable<Booking>>(), It.IsAny<string>()))
                .Returns<IQueryable<Booking>, string>((query, orderBy) => query);

            _sortHelperFactoryMock = new Mock<ISortHelperFactory>();
            _sortHelperFactoryMock
                .Setup(x => x.Create<Booking>())
                .Returns(_sortHelperMock.Object);

            _bookingService = new BookingServiceClass(
                _dbContext,
                _mapper,
                _sortHelperFactoryMock.Object,
                _publishEndpointMock.Object);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region GetBookingsAsync Tests

        [Fact]
        public async Task GetBookingsAsync_AsRegularUser_WithoutUserId_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var parameters = BookingTestData.CreateBookingParameters();

            // Act
            Func<Task> act = async () => await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: false);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("UserId is required.");
        }

        [Fact]
        public async Task GetBookingsAsync_AsRegularUser_ReturnsOnlyOwnBookings()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var otherUserId = BookingTestData.CreateUserId();

            var userBookings = BookingTestData.CreateBookingList(2, userId);
            var otherBookings = BookingTestData.CreateBookingList(1, otherUserId);

            await _dbContext.Bookings.AddRangeAsync(userBookings);
            await _dbContext.Bookings.AddRangeAsync(otherBookings);
            await _dbContext.SaveChangesAsync();

            var parameters = new BookingParameters
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(b => b.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task GetBookingsAsync_AsAdmin_WithoutUserId_ReturnsAllBookings()
        {
            // Arrange
            var bookings = BookingTestData.CreateBookingList(5);
            await _dbContext.Bookings.AddRangeAsync(bookings);
            await _dbContext.SaveChangesAsync();

            var parameters = BookingTestData.CreateBookingParameters();

            // Act
            var result = await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: true);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
        }

        [Fact]
        public async Task GetBookingsAsync_AsAdmin_WithUserId_ReturnsFilteredBookings()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var userBookings = BookingTestData.CreateBookingList(2, userId);
            var otherBookings = BookingTestData.CreateBookingList(3);

            await _dbContext.Bookings.AddRangeAsync(userBookings);
            await _dbContext.Bookings.AddRangeAsync(otherBookings);
            await _dbContext.SaveChangesAsync();

            var parameters = new BookingParameters
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: true);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(b => b.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task GetBookingsAsync_WithSearchTerm_ReturnsMatchingBookings()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking1 = BookingTestData.CreateValidBooking(userId);
            booking1.PhoneNumber = "+380501111111";
            booking1.Comment = "Special request";

            var booking2 = BookingTestData.CreateValidBooking(userId);
            booking2.PhoneNumber = "+380502222222";
            booking2.Comment = "Regular booking";

            await _dbContext.Bookings.AddRangeAsync(booking1, booking2);
            await _dbContext.SaveChangesAsync();

            var parameters = new BookingParameters
            {
                UserId = userId,
                SearchTerm = "Special",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Comment.Should().Contain("Special");
        }

        [Fact]
        public async Task GetBookingsAsync_WithDateRange_ReturnsMatchingBookings()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();

            var booking1 = BookingTestData.CreateValidBooking(userId, DateTime.UtcNow.AddDays(5));
            var booking2 = BookingTestData.CreateValidBooking(userId, DateTime.UtcNow.AddDays(10));
            var booking3 = BookingTestData.CreateValidBooking(userId, DateTime.UtcNow.AddDays(20));

            await _dbContext.Bookings.AddRangeAsync(booking1, booking2, booking3);
            await _dbContext.SaveChangesAsync();

            var parameters = new BookingParameters
            {
                UserId = userId,
                MinBookingDate = DateTime.UtcNow.AddDays(4),
                MaxBookingDate = DateTime.UtcNow.AddDays(15),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _bookingService.GetBookingsAsync(parameters, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        #endregion

        #region GetBookingByIdAsync Tests

        [Fact]
        public async Task GetBookingByIdAsync_AsRegularUser_WithOwnBooking_ReturnsBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateValidBooking(userId);

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(booking.Id);
            result.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetBookingByIdAsync_AsRegularUser_WithOtherUsersBooking_ReturnsNull()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var otherUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateValidBooking(otherUserId);

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBookingByIdAsync_AsAdmin_WithAnyBooking_ReturnsBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var adminUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateValidBooking(userId);

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, adminUserId, isAdminOrManager: true);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(booking.Id);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(nonExistentId, userId, isAdminOrManager: false);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithHookahs_ReturnsBookingWithHookahs()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateValidBooking(userId);
            booking.Hookahs.Add(BookingTestData.CreateValidHookahBooking(booking.Id));
            booking.Hookahs.Add(BookingTestData.CreateValidHookahBooking(booking.Id));

            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result!.Hookahs.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetBookingByIdAsync_WithHookahsIncludingUkrainianFields_ReturnsAllFields()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateValidBooking(userId);

            var hookah = new HookahBooking
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ProductId = BookingTestData.CreateProductId(),
                ProductName = "Fresh Mint",
                ProductNameUk = "Свіжа м'ята",
                TobaccoFlavor = "Mint",
                TobaccoFlavorUk = "М'ята",
                Strength = HookahStrength.Light,
                ServeAfterMinutes = 15,
                Notes = "Special request",
                PriceSnapshot = 250m
            };

            booking.Hookahs.Add(hookah);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result!.Hookahs.Should().HaveCount(1);

            var resultHookah = result.Hookahs[0];
            resultHookah.ProductId.Should().Be(hookah.ProductId);
            resultHookah.ProductName.Should().Be("Fresh Mint");
            resultHookah.ProductNameUk.Should().Be("Свіжа м'ята");
            resultHookah.TobaccoFlavor.Should().Be("Mint");
            resultHookah.TobaccoFlavorUk.Should().Be("М'ята");
            resultHookah.Strength.Should().Be("Light");
            resultHookah.ServeAfterMinutes.Should().Be(15);
            resultHookah.Notes.Should().Be("Special request");
            resultHookah.PriceSnapshot.Should().Be(250m);
        }

        [Fact]
        public async Task GetBookingByIdAsync_PastBooking_ComputedPropertiesCorrect()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            result.Should().NotBeNull();
            result!.IsUpcoming.Should().BeFalse();
            result.IsPast.Should().BeTrue();
            result.CanBeCancelled.Should().BeFalse();
            result.CanBeModified.Should().BeFalse();
        }

        #endregion

        #region CreateBookingAsync Tests

        [Fact]
        public async Task CreateBookingAsync_WithValidData_CreatesBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto();

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Status.Should().Be("Pending");
            result.Adults.Should().Be(dto.Adults);
            result.Children.Should().Be(dto.Children);

            var savedBooking = await _dbContext.Bookings.FindAsync(result.Id);
            savedBooking.Should().NotBeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<BookingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_WithHookahs_CreatesBookingWithHookahs()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateBookingWithHookahs(2);

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Hookahs.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateBookingAsync_WithTimeOutsideWorkingHours_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto(
                BookingTestData.GetBookingDateTimeOutsideWorkingHours());

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Bookings are only available between 10:00 and 23:00");
        }

        [Fact]
        public async Task CreateBookingAsync_WithDateTooFarInFuture_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto(
                BookingTestData.GetBookingDateTimeTooFarInFuture());

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Bookings can only be made up to 60 days in advance");
        }

        [Fact]
        public async Task CreateBookingAsync_WithTooManyGuests_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto();
            dto.Adults = 30;
            dto.Children = 25; // Total 55, exceeds limit of 50

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Total number of guests cannot exceed 50");
        }

        [Fact]
        public async Task CreateBookingAsync_WithNoGuests_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto();
            dto.Adults = 0;
            dto.Children = 0;

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("At least one guest is required");
        }

        [Fact]
        public async Task CreateBookingAsync_WithTooManyHookahs_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateBookingWithHookahs(6); // Exceeds limit of 5

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot add more than 5 hookahs per booking");
        }

        [Fact]
        public async Task CreateBookingAsync_WithHookahServeTimeExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateBookingWithHookahs(1);
            dto.Hookahs![0].ServeAfterMinutes = 250; // Exceeds 240 minutes limit

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Hookah serve time cannot exceed 240 minutes (4 hours)");
        }

        [Fact]
        public async Task CreateBookingAsync_WithConflictingBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var existingBookingTime = BookingTestData.GetValidBookingDateTime();

            var existingBooking = BookingTestData.CreateValidBooking(userId, existingBookingTime);
            existingBooking.Zone = BookingZone.MainHall;
            await _dbContext.Bookings.AddAsync(existingBooking);
            await _dbContext.SaveChangesAsync();

            var dto = BookingTestData.CreateValidBookingCreateDto(
                BookingTestData.GetBookingDateTimeWithConflict(existingBookingTime));
            dto.Zone = "MainHall";

            // Act
            Func<Task> act = async () => await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*time slot is already booked*");
        }

        [Fact]
        public async Task CreateBookingAsync_WithDifferentZone_DoesNotConflict()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var bookingTime = BookingTestData.GetValidBookingDateTime();

            var existingBooking = BookingTestData.CreateValidBooking(userId, bookingTime);
            existingBooking.Zone = BookingZone.MainHall;
            await _dbContext.Bookings.AddAsync(existingBooking);
            await _dbContext.SaveChangesAsync();

            var dto = BookingTestData.CreateValidBookingCreateDto(bookingTime);
            dto.Zone = "VIP"; // Different zone

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Zone.Should().Be("VIP");
        }

        [Fact]
        public async Task CreateBookingAsync_WithCancelledBookingInSlot_DoesNotConflict()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var bookingTime = BookingTestData.GetValidBookingDateTime();

            var cancelledBooking = BookingTestData.CreateCancelledBooking(userId);
            cancelledBooking.BookingDateTime = bookingTime;
            cancelledBooking.Zone = BookingZone.MainHall;
            await _dbContext.Bookings.AddAsync(cancelledBooking);
            await _dbContext.SaveChangesAsync();

            var dto = BookingTestData.CreateValidBookingCreateDto(bookingTime);
            dto.Zone = "MainHall";

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateBookingAsync_WithHookahsWithProductId_StoresProductId()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var productId = BookingTestData.CreateProductId();
            var dto = BookingTestData.CreateValidBookingCreateDto();

            dto.Hookahs = new List<HookahBookingDto>
            {
                new HookahBookingDto
                {
                    ProductId = productId,
                    ProductName = "Test Hookah",
                    ProductNameUk = "Тестовий кальян",
                    TobaccoFlavor = "Mint",
                    TobaccoFlavorUk = "М'ята",
                    Strength = "Medium",
                    PriceSnapshot = 250m
                }
            };

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Hookahs.Should().HaveCount(1);
            result.Hookahs[0].ProductId.Should().Be(productId);
            result.Hookahs[0].ProductName.Should().Be("Test Hookah");
            result.Hookahs[0].ProductNameUk.Should().Be("Тестовий кальян");
            result.Hookahs[0].TobaccoFlavorUk.Should().Be("М'ята");
            result.Hookahs[0].PriceSnapshot.Should().Be(250m);
        }

        [Fact]
        public async Task CreateBookingAsync_WithHookahsWithoutProductId_CreatesSuccessfully()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateValidBookingCreateDto();

            dto.Hookahs = new List<HookahBookingDto>
            {
                new HookahBookingDto
                {
                    ProductId = null,
                    ProductName = "Custom Hookah",
                    TobaccoFlavor = "Vanilla",
                    Strength = "Light",
                    PriceSnapshot = 200m
                }
            };

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Hookahs.Should().HaveCount(1);
            result.Hookahs[0].ProductId.Should().BeNull();
            result.Hookahs[0].ProductName.Should().Be("Custom Hookah");
        }

        [Fact]
        public async Task CreateBookingAsync_ComputedProperties_CalculatedCorrectly()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var dto = BookingTestData.CreateBookingWithHookahs(3);
            dto.Adults = 4;
            dto.Children = 2;

            // Act
            var result = await _bookingService.CreateBookingAsync(userId, dto);

            // Assert
            result.TotalGuests.Should().Be(6);
            result.HasHookahs.Should().BeTrue();
            result.HookahCount.Should().Be(3);
            result.IsUpcoming.Should().BeTrue();
            result.IsPast.Should().BeFalse();
            result.CanBeCancelled.Should().BeTrue();
            result.CanBeModified.Should().BeTrue();
        }

        #endregion

        #region UpdateBookingAsync Tests

        [Fact]
        public async Task UpdateBookingAsync_WithValidData_UpdatesBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreatePendingBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            var updateDto = BookingTestData.CreateValidBookingUpdateDto();
            updateDto.Adults = 5;
            updateDto.Comment = "Updated comment";

            // Act
            await _bookingService.UpdateBookingAsync(booking.Id, updateDto);

            // Assert
            var updatedBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.Adults.Should().Be(5);
            updatedBooking.Comment.Should().Be("Updated comment");
            updatedBooking.UpdatedAt.Should().NotBeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<BookingUpdatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateBookingAsync_WithNonExistentBooking_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = BookingTestData.CreateValidBookingUpdateDto();

            // Act
            Func<Task> act = async () => await _bookingService.UpdateBookingAsync(nonExistentId, updateDto);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Booking not found");
        }

        [Fact]
        public async Task UpdateBookingAsync_OnCancelledBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCancelledBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            var updateDto = BookingTestData.CreateValidBookingUpdateDto();

            // Act
            Func<Task> act = async () => await _bookingService.UpdateBookingAsync(booking.Id, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot modify a cancelled booking");
        }

        [Fact]
        public async Task UpdateBookingAsync_OnCompletedBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            var updateDto = BookingTestData.CreateValidBookingUpdateDto();

            // Act
            Func<Task> act = async () => await _bookingService.UpdateBookingAsync(booking.Id, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot modify a completed booking");
        }

        [Fact]
        public async Task UpdateBookingAsync_WithConflictingTime_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var existingBookingTime = BookingTestData.GetValidBookingDateTime();

            var existingBooking = BookingTestData.CreatePendingBooking(userId);
            existingBooking.BookingDateTime = existingBookingTime;
            existingBooking.Zone = BookingZone.MainHall;

            var bookingToUpdate = BookingTestData.CreatePendingBooking(userId);
            bookingToUpdate.BookingDateTime = existingBookingTime.AddHours(2);
            bookingToUpdate.Zone = BookingZone.MainHall;

            await _dbContext.Bookings.AddRangeAsync(existingBooking, bookingToUpdate);
            await _dbContext.SaveChangesAsync();

            var conflictingTime = BookingTestData.GetBookingDateTimeWithConflict(existingBookingTime);

            var updateDto = new BookingUpdateDto
            {
                BookingDateTime = conflictingTime,
                Zone = "MainHall",
                Adults = bookingToUpdate.Adults,
                Children = bookingToUpdate.Children
            };

            // Act
            Func<Task> act = async () => await _bookingService.UpdateBookingAsync(bookingToUpdate.Id, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*time slot is already booked*");
        }

        #endregion

        #region CancelBookingAsync Tests

        [Fact]
        public async Task CancelBookingAsync_WithPendingBooking_CancelsBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreatePendingBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            await _bookingService.CancelBookingAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            var cancelledBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            cancelledBooking.Should().NotBeNull();
            cancelledBooking!.Status.Should().Be(BookingStatus.Cancelled);
            cancelledBooking.UpdatedAt.Should().NotBeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<BookingCancelledEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_AsRegularUser_WithOtherUsersBooking_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var otherUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreatePendingBooking(otherUserId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.CancelBookingAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Booking not found");
        }

        [Fact]
        public async Task CancelBookingAsync_AsAdmin_WithAnyBooking_CancelsBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var adminUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreatePendingBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            await _bookingService.CancelBookingAsync(booking.Id, adminUserId, isAdminOrManager: true);

            // Assert
            var cancelledBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            cancelledBooking.Should().NotBeNull();
            cancelledBooking!.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_OnAlreadyCancelledBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCancelledBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.CancelBookingAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Booking is already cancelled");
        }

        [Fact]
        public async Task CancelBookingAsync_OnCompletedBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.CancelBookingAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot cancel a completed booking");
        }

        [Fact]
        public async Task CancelBookingAsync_OnNoShowBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateNoShowBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.CancelBookingAsync(booking.Id, userId, isAdminOrManager: false);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot cancel a no-show booking");
        }

        #endregion

        #region DeleteBookingAsync Tests

        [Fact]
        public async Task DeleteBookingAsync_WithCompletedBooking_DeletesBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            await _bookingService.DeleteBookingAsync(booking.Id, userId, isAdmin: false);

            // Assert
            var deletedBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            deletedBooking.Should().BeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<BookingDeletedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteBookingAsync_WithActivePendingBooking_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreatePendingBooking(userId);
            booking.BookingDateTime = DateTime.UtcNow.AddDays(1); // Future booking
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.DeleteBookingAsync(booking.Id, userId, isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete an active booking. Please cancel it first.");
        }

        [Fact]
        public async Task DeleteBookingAsync_AsRegularUser_WithOtherUsersBooking_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var otherUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(otherUserId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _bookingService.DeleteBookingAsync(booking.Id, userId, isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Booking not found");
        }

        [Fact]
        public async Task DeleteBookingAsync_AsAdmin_WithAnyBooking_DeletesBooking()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var adminUserId = BookingTestData.CreateUserId();
            var booking = BookingTestData.CreateCompletedBooking(userId);
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            // Act
            await _bookingService.DeleteBookingAsync(booking.Id, adminUserId, isAdmin: true);

            // Assert
            var deletedBooking = await _dbContext.Bookings.FindAsync(booking.Id);
            deletedBooking.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBookingAsync_WithNonExistentBooking_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = BookingTestData.CreateUserId();
            var nonExistentId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _bookingService.DeleteBookingAsync(nonExistentId, userId, isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Booking not found");
        }

        #endregion
    }
}