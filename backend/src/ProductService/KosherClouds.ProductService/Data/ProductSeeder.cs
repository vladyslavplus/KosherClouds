namespace KosherClouds.ProductService.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


    public static class ProductSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            await dbContext.Database.MigrateAsync(); 

            if (!await dbContext.Products.AnyAsync())
            {
                var kugelId = new Guid("1a3b5c7d-9e1f-4a2b-3c4d-5e6f7a8b9c0d");
                var falafelSetId = new Guid("2b4c6d8e-0f2a-5b3c-4d5e-6f7a8b9c0d1e");
                var hookahId = new Guid("5c1f0d3b-9g5e-4f0a-97c4-91c7621f5812");

                var kugel = new Product
                {
                    Id = kugelId,
                    Name = "Картопляний Кугель (Сідінг)",
                    Description = "Традиційна запіканка з тертої картоплі, парве. Сідингові дані.",
                    Price = 115.00m,
                    Category = ProductCategory.Dish,
                    SubCategory = "Vegetarian",
                    IsAvailable = true,
                    Rating = 4.8,
                    RatingCount = 95,
                    Ingredients = new List<string> { "Картопля", "Яйця", "Цибуля", "Олія" },
                    Allergens = new List<string> { "Яйця" },
                    Photos = new List<string> { "https://example.com/kugel_seeding.jpg" },
                    HookahDetails = null 
                };

                var falafelSet = new Product
                {
                    Id = falafelSetId,
                    Name = "Великий Фалафель Сет (Сідінг)",
                    Description = "Фалафель з хумусом, пітою та соусами. Сідингові дані.",
                    Price = 240.00m,
                    Category = ProductCategory.Set,
                    SubCategory = "Vegetarian",
                    IsAvailable = true,
                    Rating = 4.6,
                    RatingCount = 150,
                    Ingredients = new List<string> { "Нут", "Петрушка", "Тхіна" },
                    Allergens = new List<string> { "Кунжут (тхіна)" },
                    Photos = new List<string> { "https://example.com/falafel_set_seeding.jpg" },
                    HookahDetails = null
                };

                var tropicalHookah = new Product
                {
                    Id = hookahId,
                    Name = "Тропічний Мікс (Кальян, Сідінг)",
                    Description = "Соковита суміш смаків манго, маракуї та кокосу.",
                    Price = 380.00m,
                    Category = ProductCategory.Hookah,
                    SubCategory = "Fruit",
                    IsAvailable = true,
                    Rating = 4.9,
                    RatingCount = 85,
                    Ingredients = new List<string> { "Тютюн", "Кокосове вугілля" },
                    Allergens = new List<string>(),
                    Photos = new List<string> { "https://example.com/hookah_tropical_seeding.jpg" },
                    HookahDetails = new HookahDetails
                    {
                        TobaccoFlavor = "Манго та Маракуя",
                        Strength = HookahStrength.Medium,
                        BowlType = "Глина",
                        AdditionalParams = new Dictionary<string, string> { { "Основа", "Вода" }, { "Додаток", "Апельсин" } }
                    }
                };

                dbContext.Products.Add(kugel);
                dbContext.Products.Add(falafelSet);
                dbContext.Products.Add(tropicalHookah);

                await dbContext.SaveChangesAsync();
                Console.WriteLine("✅ Початкові дані продуктів успішно додано.");
            }
            else
            {
                 Console.WriteLine("ℹ️ Таблиця Products вже містить дані. Сідинг пропущено.");
            }
        }
    }