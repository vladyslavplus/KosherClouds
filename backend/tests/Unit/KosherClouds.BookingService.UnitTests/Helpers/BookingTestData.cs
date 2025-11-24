using Bogus;
using KosherClouds.BookingService.DTOs;
using KosherClouds.BookingService.Entities;
using KosherClouds.BookingService.Parameters;

namespace KosherClouds.BookingService.UnitTests.Helpers
{
    public static class BookingTestData
    {
        private static readonly Faker _faker = new Faker();

        public static Guid CreateUserId() => Guid.NewGuid();

        public static Booking CreateValidBooking(Guid? userId = null, DateTime? bookingDateTime = null)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                BookingDateTime = bookingDateTime ?? GetValidBookingDateTime(),
                Adults = _faker.Random.Int(1, 10),
                Children = _faker.Random.Int(0, 5),
                Zone = BookingZone.MainHall,
                Status = BookingStatus.Pending,
                PhoneNumber = "+380501234567",
                Comment = _faker.Lorem.Sentence(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Hookahs = new List<HookahBooking>()
            };

            return booking;
        }

        public static Booking CreatePendingBooking(Guid userId)
        {
            var booking = CreateValidBooking(userId);
            booking.Status = BookingStatus.Pending;
            return booking;
        }

        public static Booking CreateConfirmedBooking(Guid userId)
        {
            var booking = CreateValidBooking(userId);
            booking.Status = BookingStatus.Confirmed;
            return booking;
        }

        public static Booking CreateCancelledBooking(Guid userId)
        {
            var booking = CreateValidBooking(userId);
            booking.Status = BookingStatus.Cancelled;
            return booking;
        }

        public static Booking CreateCompletedBooking(Guid userId)
        {
            var booking = CreateValidBooking(userId);
            booking.Status = BookingStatus.Completed;
            booking.BookingDateTime = DateTime.UtcNow.AddDays(-1);
            return booking;
        }

        public static Booking CreateNoShowBooking(Guid userId)
        {
            var booking = CreateValidBooking(userId);
            booking.Status = BookingStatus.NoShow;
            booking.BookingDateTime = DateTime.UtcNow.AddDays(-1);
            return booking;
        }

        public static BookingCreateDto CreateValidBookingCreateDto(DateTime? bookingDateTime = null)
        {
            return new BookingCreateDto
            {
                BookingDateTime = bookingDateTime ?? GetValidBookingDateTime(),
                Adults = _faker.Random.Int(1, 10),
                Children = _faker.Random.Int(0, 5),
                Zone = "MainHall",
                PhoneNumber = "+380501234567",
                Comment = _faker.Lorem.Sentence(),
                Hookahs = null
            };
        }

        public static BookingCreateDto CreateBookingWithHookahs(int hookahCount = 2)
        {
            var dto = CreateValidBookingCreateDto();
            dto.Hookahs = CreateHookahList(hookahCount);
            return dto;
        }

        public static BookingUpdateDto CreateValidBookingUpdateDto()
        {
            return new BookingUpdateDto
            {
                BookingDateTime = GetValidBookingDateTime(),
                Adults = _faker.Random.Int(1, 10),
                Children = _faker.Random.Int(0, 5),
                Zone = "VIP",
                PhoneNumber = "+380501234567",
                Comment = "Updated comment",
                Status = "Confirmed"
            };
        }

        public static HookahBookingDto CreateValidHookahDto()
        {
            return new HookahBookingDto
            {
                TobaccoFlavor = _faker.Commerce.ProductName(),
                Strength = "Medium",
                ServeAfterMinutes = _faker.Random.Int(0, 120),
                Notes = _faker.Lorem.Sentence()
            };
        }

        public static List<HookahBookingDto> CreateHookahList(int count)
        {
            var hookahs = new List<HookahBookingDto>();
            for (int i = 0; i < count; i++)
            {
                hookahs.Add(CreateValidHookahDto());
            }
            return hookahs;
        }

        public static HookahBooking CreateValidHookahBooking(Guid bookingId)
        {
            return new HookahBooking
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                TobaccoFlavor = _faker.Commerce.ProductName(),
                Strength = HookahStrength.Medium,
                ServeAfterMinutes = _faker.Random.Int(0, 120),
                Notes = _faker.Lorem.Sentence()
            };
        }

        public static List<Booking> CreateBookingList(int count, Guid? userId = null)
        {
            var bookings = new List<Booking>();
            for (int i = 0; i < count; i++)
            {
                bookings.Add(CreateValidBooking(userId));
            }
            return bookings;
        }

        public static BookingParameters CreateBookingParameters()
        {
            return new BookingParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
        }

        public static DateTime GetValidBookingDateTime()
        {
            var tomorrow = DateTime.UtcNow.AddDays(1);
            return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 14, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime GetBookingDateTimeOutsideWorkingHours()
        {
            var tomorrow = DateTime.UtcNow.AddDays(1);
            return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 9, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime GetBookingDateTimeTooFarInFuture()
        {
            return DateTime.UtcNow.AddDays(70);
        }

        public static DateTime GetPastBookingDateTime()
        {
            return DateTime.UtcNow.AddDays(-1);
        }

        public static DateTime GetBookingDateTimeWithConflict(DateTime existingBooking)
        {
            return existingBooking.AddMinutes(15);
        }

        public static DateTime GetBookingDateTimeWithoutConflict(DateTime existingBooking)
        {
            return existingBooking.AddMinutes(45);
        }
    }
}
