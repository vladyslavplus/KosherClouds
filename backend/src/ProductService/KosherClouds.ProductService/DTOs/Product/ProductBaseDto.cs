namespace KosherClouds.ProductService.DTOs.Product;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.Entities.Enums;

    public abstract class ProductBaseDto
    {
        [Required(ErrorMessage = "Назва продукту є обов'язковою.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис продукту є обов'язковим.")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ціна є обов'язковою.")]
        [Range(0.01, 9999.99, ErrorMessage = "Ціна має бути позитивною.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Категорія є обов'язковою.")]
        public ProductCategory Category { get; set; }
        
        public string? SubCategory { get; set; }

        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Allergens { get; set; } = new List<string>();
        public List<string> Photos { get; set; } = new List<string>();

        public bool IsAvailable { get; set; } = true;

        public HookahDetailsDto? HookahDetails { get; set; }
    }