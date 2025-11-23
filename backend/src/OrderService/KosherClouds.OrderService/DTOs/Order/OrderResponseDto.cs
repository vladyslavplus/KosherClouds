using KosherClouds.OrderService.DTOs.OrderItem;
using KosherClouds.OrderService.Entities;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();
    }
}