namespace KosherClouds.ProductService.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadPhotoAsync(IFormFile file, string folder = "kosher-clouds/products");
        Task<List<string>> UploadMultiplePhotosAsync(IFormFileCollection files, string folder = "kosher-clouds/products");
        Task<string> UploadPhotoFromUrlAsync(string url, string folder = "kosher-clouds/products");
        Task<List<string>> UploadMultiplePhotosFromUrlsAsync(List<string> urls, string folder = "kosher-clouds/products");
        Task<bool> DeletePhotoAsync(string publicId);
        string ExtractPublicIdFromUrl(string url);
    }
}
