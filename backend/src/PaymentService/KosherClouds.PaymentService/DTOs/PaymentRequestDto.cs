using System.ComponentModel.DataAnnotations;

namespace KosherClouds.PaymentService.DTOs
{
    public class PaymentRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "usd";

        [Required]
        [EmailAddress]
        public string ReceiptEmail { get; set; } = string.Empty;
    }
}
