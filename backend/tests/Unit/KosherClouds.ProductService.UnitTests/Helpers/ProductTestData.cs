using Bogus;
using KosherClouds.Common.Seed;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;

namespace KosherClouds.ProductService.UnitTests.Helpers
{
    public static class ProductTestData
    {
        private static readonly Faker _faker = new Faker();

        public static Product CreateValidProduct()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Price = decimal.Parse(_faker.Commerce.Price()),
                DiscountPrice = null,
                IsPromotional = false,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                IsVegetarian = _faker.Random.Bool(),
                Ingredients = new List<string> { "Ingredient 1", "Ingredient 2" },
                Allergens = new List<string>(),
                Photos = new List<string> { "photo1.jpg" },
                IsAvailable = true,
                Rating = 4.5,
                RatingCount = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static List<Product> CreateProductList(int count)
        {
            var products = new List<Product>();
            for (int i = 0; i < count; i++)
            {
                products.Add(CreateValidProduct());
            }
            return products;
        }

        public static ProductCreateRequest CreateValidProductCreateRequest()
        {
            return new ProductCreateRequest
            {
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Price = decimal.Parse(_faker.Commerce.Price()),
                Category = ProductCategory.Dish,
                IsVegetarian = false,
                Ingredients = new List<string> { "Test Ingredient" },
                Allergens = new List<string>(),
                Photos = new List<string> { "test.jpg" }
            };
        }

        public static ProductUpdateRequest CreateValidProductUpdateRequest()
        {
            return new ProductUpdateRequest
            {
                Name = _faker.Commerce.ProductName(),
                Price = decimal.Parse(_faker.Commerce.Price()),
                IsAvailable = true
            };
        }

        public static Product CreateProductWithKnownId(Guid id)
        {
            var product = CreateValidProduct();
            product.Id = id;
            return product;
        }

        public static Product CreateKugelProduct()
        {
            return new Product
            {
                Id = SharedSeedData.ProductKugelId,
                Name = SharedSeedData.ProductKugelName,
                Description = "Traditional Jewish noodle casserole",
                Price = SharedSeedData.ProductKugelPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.5,
                RatingCount = 25,
                Ingredients = new List<string> { "Noodles", "Eggs", "Cottage Cheese" },
                Allergens = new List<string> { "Gluten", "Dairy", "Eggs" },
                Photos = new List<string> { "kugel.jpg" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateFalafelProduct()
        {
            return new Product
            {
                Id = SharedSeedData.ProductFalafelSetId,
                Name = SharedSeedData.ProductFalafelSetName,
                Description = "Crispy chickpea balls with tahini sauce",
                Price = SharedSeedData.ProductFalafelSetPrice,
                Category = ProductCategory.Set,
                SubCategory = "Appetizer",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.8,
                RatingCount = 42,
                Ingredients = new List<string> { "Chickpeas", "Herbs", "Spices" },
                Allergens = new List<string> { "Sesame" },
                Photos = new List<string> { "falafel.jpg" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateHookahProduct()
        {
            return new Product
            {
                Id = SharedSeedData.ProductHookahTropicalId,
                Name = SharedSeedData.ProductHookahTropicalName,
                Description = "Tropical flavored hookah experience",
                Price = SharedSeedData.ProductHookahTropicalPrice,
                Category = ProductCategory.Hookah,
                IsAvailable = true,
                Rating = 4.6,
                RatingCount = 18,
                Photos = new List<string> { "hookah_tropical.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Tropical Mix",
                    Strength = HookahStrength.Medium,
                    BowlType = "Clay",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Duration", "60 minutes" },
                        { "Type", "Premium" }
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}