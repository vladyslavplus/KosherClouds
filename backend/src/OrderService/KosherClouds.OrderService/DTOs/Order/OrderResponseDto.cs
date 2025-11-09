using KosherClouds.OrderService.DTOs.OrderItem;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
    }
}