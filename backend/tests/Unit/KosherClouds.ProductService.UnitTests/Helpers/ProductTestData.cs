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
                NameUk = null,
                Description = _faker.Commerce.ProductDescription(),
                DescriptionUk = null,
                Price = decimal.Parse(_faker.Commerce.Price()),
                DiscountPrice = null,
                IsPromotional = false,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                SubCategoryUk = null,
                IsVegetarian = _faker.Random.Bool(),
                Ingredients = new List<string> { "Ingredient 1", "Ingredient 2" },
                IngredientsUk = new List<string>(),
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
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
                NameUk = "Кугель",
                Description = "Traditional Jewish noodle casserole",
                DescriptionUk = "Традиційна єврейська запіканка з локшини",
                Price = SharedSeedData.ProductKugelPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                SubCategoryUk = "Основна страва",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.5,
                RatingCount = 25,
                Ingredients = new List<string> { "Noodles", "Eggs", "Cottage Cheese" },
                IngredientsUk = new List<string> { "Локшина", "Яйця", "Сир" },
                Allergens = new List<string> { "Gluten", "Dairy", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Молочні продукти", "Яйця" },
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
                NameUk = "Фалафель сет",
                Description = "Crispy chickpea balls with tahini sauce",
                DescriptionUk = "Хрусткі кульки з нуту з соусом тахіні",
                Price = SharedSeedData.ProductFalafelSetPrice,
                Category = ProductCategory.Set,
                SubCategory = "Appetizer",
                SubCategoryUk = "Закуска",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 4.8,
                RatingCount = 42,
                Ingredients = new List<string> { "Chickpeas", "Herbs", "Spices" },
                IngredientsUk = new List<string> { "Нут", "Трави", "Спеції" },
                Allergens = new List<string> { "Sesame" },
                AllergensUk = new List<string> { "Кунжут" },
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
                NameUk = "Кальян Тропічний",
                Description = "Tropical flavored hookah experience",
                DescriptionUk = "Тропічний кальян з незабутнім смаком",
                Price = SharedSeedData.ProductHookahTropicalPrice,
                Category = ProductCategory.Hookah,
                IsAvailable = true,
                Rating = 4.6,
                RatingCount = 18,
                Photos = new List<string> { "hookah_tropical.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Tropical Mix",
                    TobaccoFlavorUk = "Тропічний мікс",
                    Strength = HookahStrength.Medium,
                    BowlType = "Clay",
                    BowlTypeUk = "Глиняна",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Duration", "60 minutes" },
                        { "Type", "Premium" }
                    },
                    AdditionalParamsUk = new Dictionary<string, string>
                    {
                        { "Duration", "60 хвилин" },
                        { "Type", "Преміум" }
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateProductWithUkrainianFields()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Challah Bread",
                NameUk = "Хала",
                Description = "Traditional braided bread",
                DescriptionUk = "Традиційний плетений хліб",
                Price = 80m,
                Category = ProductCategory.Dish,
                SubCategory = "Bakery",
                SubCategoryUk = "Випічка",
                IsVegetarian = true,
                IsAvailable = true,
                Ingredients = new List<string> { "Flour", "Eggs", "Honey" },
                IngredientsUk = new List<string> { "Борошно", "Яйця", "Мед" },
                Allergens = new List<string> { "Gluten", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Яйця" },
                Photos = new List<string> { "challah.jpg" },
                Rating = 4.7,
                RatingCount = 35,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreatePromotionalProduct()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Special Friday Dinner",
                NameUk = "Спеціальна П'ятнична Вечеря",
                Description = "Limited time offer for Shabbat dinner",
                DescriptionUk = "Обмежена пропозиція для вечері Шабат",
                Price = 200m,
                DiscountPrice = 150m,
                IsPromotional = true,
                Category = ProductCategory.Set,
                SubCategory = "Special Offer",
                SubCategoryUk = "Спеціальна пропозиція",
                IsVegetarian = false,
                IsAvailable = true,
                Photos = new List<string> { "friday_dinner.jpg", "friday_dinner_2.jpg" },
                Rating = 4.9,
                RatingCount = 120,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateProductWithMultiplePhotos()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Deluxe Mezze Platter",
                Description = "Variety of Middle Eastern appetizers",
                Price = 180m,
                Category = ProductCategory.Set,
                IsVegetarian = true,
                IsAvailable = true,
                Photos = new List<string>
                {
                    "mezze_1.jpg",
                    "mezze_2.jpg",
                    "mezze_3.jpg",
                    "mezze_4.jpg"
                },
                Rating = 4.8,
                RatingCount = 67,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateUnavailableProduct()
        {
            var product = CreateValidProduct();
            product.IsAvailable = false;
            return product;
        }

        public static Product CreateVegetarianProduct()
        {
            var product = CreateValidProduct();
            product.IsVegetarian = true;
            product.Name = "Vegetarian Shakshuka";
            return product;
        }

        public static Product CreateNonVegetarianProduct()
        {
            var product = CreateValidProduct();
            product.IsVegetarian = false;
            product.Name = "Beef Kebab";
            return product;
        }

        public static Product CreateProductWithPrice(decimal price)
        {
            var product = CreateValidProduct();
            product.Price = price;
            return product;
        }

        public static Product CreateOldProduct(int daysOld)
        {
            var product = CreateValidProduct();
            product.CreatedAt = DateTime.UtcNow.AddDays(-daysOld);
            return product;
        }

        public static Product CreateDessertProduct()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Honey Cake",
                NameUk = "Медовий торт",
                Description = "Traditional Jewish honey cake",
                DescriptionUk = "Традиційний єврейський медовий торт",
                Price = 95m,
                Category = ProductCategory.Dessert,
                SubCategory = "Cake",
                SubCategoryUk = "Торт",
                IsVegetarian = true,
                IsAvailable = true,
                Ingredients = new List<string> { "Honey", "Flour", "Eggs", "Cinnamon" },
                IngredientsUk = new List<string> { "Мед", "Борошно", "Яйця", "Кориця" },
                Allergens = new List<string> { "Gluten", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Яйця" },
                Photos = new List<string> { "honey_cake.jpg" },
                Rating = 4.6,
                RatingCount = 88,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Product CreateDrinkProduct()
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Fresh Orange Juice",
                NameUk = "Свіжовичавлений апельсиновий сік",
                Description = "Freshly squeezed orange juice",
                DescriptionUk = "Свіжовичавлений апельсиновий сік",
                Price = 45m,
                Category = ProductCategory.Drink,
                IsVegetarian = true,
                IsAvailable = true,
                Photos = new List<string> { "orange_juice.jpg" },
                Rating = 4.4,
                RatingCount = 52,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}