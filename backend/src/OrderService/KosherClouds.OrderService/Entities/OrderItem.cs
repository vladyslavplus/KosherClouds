using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(250)]
        public string ProductNameSnapshot { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? ProductNameSnapshotUk { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal UnitPriceSnapshot { get; set; }

        [Required]
        public int Quantity { get; set; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitPriceSnapshot;

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}