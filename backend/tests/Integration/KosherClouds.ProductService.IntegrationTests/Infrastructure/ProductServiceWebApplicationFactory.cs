using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json.Serialization;
using Testcontainers.PostgreSql;

namespace KosherClouds.ProductService.IntegrationTests.Infrastructure
{
    public class ProductServiceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;

        public ProductServiceWebApplicationFactory()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("productservice_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString());
            builder.UseSetting("Jwt:Key", "SuperSecretKeyForTestingPurposesOnly12345678");
            builder.UseSetting("Jwt:Issuer", "KosherCloudsTestIssuer");
            builder.UseSetting("Jwt:Audience", "KosherCloudsTestAudience");

            builder.UseSetting("Cloudinary:CloudName", "test-cloud");
            builder.UseSetting("Cloudinary:ApiKey", "test-key");
            builder.UseSetting("Cloudinary:ApiSecret", "test-secret");

            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ProductDbContext>>();
                services.RemoveAll<ProductDbContext>();

                services.AddDbContext<ProductDbContext>(options =>
                    options.UseNpgsql(_postgresContainer.GetConnectionString()));

                services.RemoveAll<ICloudinaryService>();
                services.AddSingleton<ICloudinaryService, MockCloudinaryService>();

                services.AddMassTransitTestHarness(x =>
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                });

                services.Configure<JsonOptions>(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    public class MockCloudinaryService : ICloudinaryService
    {
        private static int _photoCounter = 0;

        public Task<string> UploadPhotoAsync(IFormFile file, string folder = "kosher-clouds/products")
        {
            var counter = Interlocked.Increment(ref _photoCounter);
            return Task.FromResult($"https://test.cloudinary.com/test-cloud/image/upload/v1/{folder}/test-photo-{counter}.jpg");
        }

        public async Task<List<string>> UploadMultiplePhotosAsync(IFormFileCollection files, string folder = "kosher-clouds/products")
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                var url = await UploadPhotoAsync(file, folder);
                urls.Add(url);
            }
            return urls;
        }

        public Task<string> UploadPhotoFromUrlAsync(string url, string folder = "kosher-clouds/products")
        {
            var counter = Interlocked.Increment(ref _photoCounter);
            return Task.FromResult($"https://test.cloudinary.com/test-cloud/image/upload/v1/{folder}/test-photo-{counter}.jpg");
        }

        public async Task<List<string>> UploadMultiplePhotosFromUrlsAsync(List<string> urls, string folder = "kosher-clouds/products")
        {
            var uploadedUrls = new List<string>();
            foreach (var url in urls)
            {
                var uploadedUrl = await UploadPhotoFromUrlAsync(url, folder);
                uploadedUrls.Add(uploadedUrl);
            }
            return uploadedUrls;
        }

        public Task<bool> DeletePhotoAsync(string publicId)
        {
            return Task.FromResult(true);
        }

        public string ExtractPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            var uploadIndex = Array.IndexOf(segments, "upload");

            if (uploadIndex >= 0 && uploadIndex + 2 < segments.Length)
            {
                var pathSegments = segments.Skip(uploadIndex + 2);
                var publicId = string.Join("/", pathSegments);

                var lastDotIndex = publicId.LastIndexOf('.');
                if (lastDotIndex > 0)
                    publicId = publicId.Substring(0, lastDotIndex);

                return publicId;
            }

            return "test-public-id";
        }
    }
}