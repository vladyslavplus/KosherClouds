using KosherClouds.ProductService.Data;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ProductService.UnitTests.Helpers
{
    public static class MockDbContextFactory
    {
        public static ProductDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ProductDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
