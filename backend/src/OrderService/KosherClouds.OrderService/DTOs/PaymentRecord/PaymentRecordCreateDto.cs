namespace KosherClouds.OrderService.DTOs.PaymentRecord;
using System.ComponentModel.DataAnnotations;


    public class PaymentRecordCreateDto
    {
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 
        
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; 
        
        [MaxLength(250)]
        public string? TransactionId { get; set; } 
    }
