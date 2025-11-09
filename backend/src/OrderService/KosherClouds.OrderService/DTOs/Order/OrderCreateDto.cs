using KosherClouds.OrderService.DTOs.OrderItem;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderCreateDto
    {
        [Required]
        public Guid UserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public List<OrderItemCreateDto> Items { get; set; } = new();
    }
}