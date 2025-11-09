namespace KosherClouds.OrderService.DTOs.OrderItem;
using System.ComponentModel.DataAnnotations;


    public class OrderItemCreateDto
    {
        
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [MaxLength(250)]
        public string ProductNameSnapshot { get; set; } = string.Empty;
        
        [Required]
        public decimal UnitPriceSnapshot { get; set; }
        
        [Required]
        public int Quantity { get; set; }
    }
