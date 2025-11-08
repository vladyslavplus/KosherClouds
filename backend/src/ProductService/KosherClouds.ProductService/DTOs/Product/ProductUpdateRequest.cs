namespace KosherClouds.ProductService.DTOs.Product;

using System.ComponentModel.DataAnnotations;
using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.Entities.Enums;


public class ProductUpdateRequest : ProductBaseDto
{
    [Range(0.0, 5.0)]
    public double? Rating { get; set; }
    public long? RatingCount { get; set; }
}