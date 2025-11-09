namespace KosherClouds.CartService.DTOs;

using System.ComponentModel.DataAnnotations;

public class CartItemAddDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int Quantity { get; set; } = 1;
}