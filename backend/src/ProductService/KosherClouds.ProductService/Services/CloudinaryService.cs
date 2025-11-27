using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using KosherClouds.ProductService.Services.Interfaces;

namespace KosherClouds.ProductService.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary credentials are not configured properly.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadPhotoAsync(IFormFile file, string folder = "kosher-clouds/products")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, and WebP are allowed.");

            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 10MB limit.");

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Width(1200)
                        .Height(1200)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                    throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error?.Message}");
                }

                _logger.LogInformation("Photo uploaded successfully: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo to Cloudinary");
                throw;
            }
        }

        public async Task<List<string>> UploadMultiplePhotosAsync(IFormFileCollection files, string folder = "kosher-clouds/products")
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                var url = await UploadPhotoAsync(file, folder);
                uploadedUrls.Add(url);
            }

            return uploadedUrls;
        }

        public async Task<string> UploadPhotoFromUrlAsync(string url, string folder = "kosher-clouds/products")
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is empty or null.");

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(url),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Width(1200)
                        .Height(1200)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload from URL failed: {Error}", uploadResult.Error?.Message);
                    throw new InvalidOperationException($"Cloudinary upload from URL failed: {uploadResult.Error?.Message}");
                }

                _logger.LogInformation("Photo uploaded from URL successfully: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo from URL to Cloudinary");
                throw;
            }
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

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Photo deleted successfully: {PublicId}", publicId);
                    return true;
                }

                _logger.LogWarning("Failed to delete photo: {PublicId}", publicId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo from Cloudinary");
                return false;
            }
        }

        public string ExtractPublicIdFromUrl(string url)
        {
            try
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

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
