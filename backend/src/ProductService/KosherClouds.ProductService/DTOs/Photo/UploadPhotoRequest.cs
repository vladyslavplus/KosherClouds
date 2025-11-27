namespace KosherClouds.ProductService.DTOs.Photo
{
    public class UploadPhotoRequest
    {
        public IFormFile? File { get; set; }
        public string? Url { get; set; }
    }
}
