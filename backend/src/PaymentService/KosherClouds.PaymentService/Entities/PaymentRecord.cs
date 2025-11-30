using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.PaymentService.Entities
{
    public class PaymentRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = "Stripe";

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed

        [MaxLength(250)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
