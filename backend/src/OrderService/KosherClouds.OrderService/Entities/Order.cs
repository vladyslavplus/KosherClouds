using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.OrderService.Entities
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        [Required]
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; } = PaymentType.OnPickup;

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}