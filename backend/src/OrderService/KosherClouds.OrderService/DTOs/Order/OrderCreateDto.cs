namespace KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.DTOs.OrderItem;
using System.ComponentModel.DataAnnotations;

    public class OrderCreateDto
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public decimal TotalAmount { get; set; } 
        
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }
