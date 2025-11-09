namespace KosherClouds.OrderService.DTOs.Order;
using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.DTOs.PaymentRecord;

    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty; 
        public decimal TotalAmount { get; set; } 
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
        public ICollection<PaymentRecordResponseDto> Payments { get; set; } = new List<PaymentRecordResponseDto>();
    }