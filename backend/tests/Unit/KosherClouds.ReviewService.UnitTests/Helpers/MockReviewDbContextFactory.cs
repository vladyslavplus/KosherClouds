using KosherClouds.ReviewService.Data;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ReviewService.UnitTests.Helpers
{
    public static class MockReviewDbContextFactory
    {
        public static ReviewDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ReviewDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ReviewDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
