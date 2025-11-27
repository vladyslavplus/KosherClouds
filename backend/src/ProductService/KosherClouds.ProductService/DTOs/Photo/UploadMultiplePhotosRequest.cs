namespace KosherClouds.ProductService.DTOs.Photo
{
    public class UploadMultiplePhotosRequest
    {
        public IFormFileCollection? Files { get; set; }
        public List<string>? Urls { get; set; }
    }
}
