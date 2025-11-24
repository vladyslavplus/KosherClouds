using KosherClouds.BookingService.Data;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.BookingService.UnitTests.Helpers
{
    public static class MockBookingDbContextFactory
    {
        public static BookingDbContext Create()
        {
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new BookingDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
