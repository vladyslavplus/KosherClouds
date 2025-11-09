namespace KosherClouds.OrderService.DTOs.Order;

using System.ComponentModel.DataAnnotations;

    public class OrderUpdateDto
    {
        public Guid? UserId { get; set; }
        
        [MaxLength(50)]
        public string? Status { get; set; }
        
        public decimal? TotalAmount { get; set; } 
        
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
    }