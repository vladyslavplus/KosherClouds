using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.OrderItem
{
    public class OrderItemCreateDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(250)]
        public string ProductNameSnapshot { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? ProductNameSnapshotUk { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPriceSnapshot { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}