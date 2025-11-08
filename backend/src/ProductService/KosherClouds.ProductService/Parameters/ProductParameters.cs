namespace KosherClouds.ProductService.Parameters;
using KosherClouds.ServiceDefaults.Helpers;


    public class ProductParameters : QueryStringParameters {
        
        public string? Category { get; set; }
        public bool IsVegetarian { get; set; } = false;

        public bool IsAvailable { get; set; } = true;
        
        
        public bool IsHookah { get; set; } = false; 

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; } = decimal.MaxValue;
        
        public string? SearchTerm { get; set; }
    }
