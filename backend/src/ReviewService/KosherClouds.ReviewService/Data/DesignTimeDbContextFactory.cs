using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KosherClouds.ReviewService.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReviewDbContext>
    {
        public ReviewDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ReviewDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ReviewDbContext(optionsBuilder.Options);
        }
    }
}
