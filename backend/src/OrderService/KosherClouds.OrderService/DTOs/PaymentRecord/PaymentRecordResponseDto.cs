namespace KosherClouds.OrderService.DTOs.PaymentRecord;

public class PaymentRecordResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; 
    public string PaymentMethod { get; set; } = string.Empty; 
    public string? TransactionId { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}