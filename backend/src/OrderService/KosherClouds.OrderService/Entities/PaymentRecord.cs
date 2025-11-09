namespace KosherClouds.OrderService.Entities;



using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore; 

    public class PaymentRecord
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 
        
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; 
        
        [MaxLength(250)]
        public string? TransactionId { get; set; } 

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        
        [Required]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
