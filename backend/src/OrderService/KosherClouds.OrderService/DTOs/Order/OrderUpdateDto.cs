using KosherClouds.OrderService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderUpdateDto
    {
        public OrderStatus? Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}