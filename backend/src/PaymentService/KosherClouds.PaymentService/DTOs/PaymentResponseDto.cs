namespace KosherClouds.PaymentService.DTOs
{
    public class PaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? ClientSecret { get; set; }
        public string Provider { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
