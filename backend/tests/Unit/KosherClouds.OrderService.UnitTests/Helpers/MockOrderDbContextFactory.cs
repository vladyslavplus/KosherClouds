using KosherClouds.OrderService.Data;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.UnitTests.Helpers
{
    public static class MockOrderDbContextFactory
    {
        public static OrderDbContext Create()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new OrderDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
