using KosherClouds.OrderService.Entities;
using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderConfirmDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; } = PaymentType.OnPickup;
    }
}
