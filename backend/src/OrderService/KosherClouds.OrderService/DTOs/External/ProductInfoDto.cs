namespace KosherClouds.OrderService.DTOs.External
{
    public class ProductInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameUk { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsAvailable { get; set; }
        public decimal ActualPrice => DiscountPrice ?? Price;
    }
}