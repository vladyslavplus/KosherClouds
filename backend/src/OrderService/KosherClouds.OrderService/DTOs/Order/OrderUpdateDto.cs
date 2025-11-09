using System.ComponentModel.DataAnnotations;

namespace KosherClouds.OrderService.DTOs.Order
{
    public class OrderUpdateDto
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}