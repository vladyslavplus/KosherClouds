namespace KosherClouds.OrderService.Entities;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 
        
        [Required]
        [Precision(18, 2)]
        public decimal TotalAmount { get; set; } 
        
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        [Required]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
    }