using KosherClouds.UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.UserService.UnitTests.Helpers
{
    public static class MockUserDbContextFactory
    {
        public static UserDbContext Create()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new UserDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
