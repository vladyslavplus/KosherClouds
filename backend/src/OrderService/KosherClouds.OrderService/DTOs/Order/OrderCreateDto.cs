using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; } = PaymentType.OnPickup;

        [Required]
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }
}