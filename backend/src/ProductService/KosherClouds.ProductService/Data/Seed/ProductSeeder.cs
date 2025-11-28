using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.Common.Seed;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.ProductService.Data.Seed;

public static class ProductSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("ProductSeeder");

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Products.AnyAsync())
        {
            logger.LogInformation("ProductSeeder: Products table already contains data. Seeding skipped.");
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Id = SharedSeedData.ProductKugelId,
                Name = SharedSeedData.ProductKugelName,
                NameUk = SharedSeedData.ProductKugelNameUk,
                Description = "Traditional Jewish baked potato pudding made with onions and eggs.",
                DescriptionUk = "Традиційний єврейський запечений картопляний пудинг з цибулею та яйцями.",
                Price = SharedSeedData.ProductKugelPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Classic",
                SubCategoryUk = "Класичні",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Potato", "Eggs", "Onion", "Oil" },
                IngredientsUk = new List<string> { "Картопля", "Яйця", "Цибуля", "Олія" },
                Allergens = new List<string> { "Eggs" },
                AllergensUk = new List<string> { "Яйця" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764233482/kugel_vhzocy.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductFalafelSetId,
                Name = SharedSeedData.ProductFalafelSetName,
                NameUk = SharedSeedData.ProductFalafelSetNameUk,
                Description = "Crispy falafel served with hummus, pita bread, and tahini sauce.",
                DescriptionUk = "Хрусткий фалафель подається з хумусом, пітою та соусом тахіні.",
                Price = SharedSeedData.ProductFalafelSetPrice,
                DiscountPrice = SharedSeedData.ProductFalafelSetDiscountPrice,
                Category = ProductCategory.Set,
                SubCategory = "Vegetarian",
                SubCategoryUk = "Вегетаріанські",
                IsVegetarian = true,
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Chickpeas", "Parsley", "Tahini", "Pita Bread" },
                IngredientsUk = new List<string> { "Нут", "Петрушка", "Тахіні", "Піта" },
                Allergens = new List<string> { "Sesame", "Gluten" },
                AllergensUk = new List<string> { "Кунжут", "Глютен" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764233482/falafel_rzczew.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductHookahTropicalId,
                Name = SharedSeedData.ProductHookahTropicalName,
                NameUk = SharedSeedData.ProductHookahTropicalNameUk,
                Description = "A rich tropical blend with mango, passion fruit, and coconut flavor.",
                DescriptionUk = "Насичена тропічна суміш зі смаком манго, маракуї та кокоса.",
                Price = SharedSeedData.ProductHookahTropicalPrice,
                DiscountPrice = SharedSeedData.ProductHookahTropicalDiscountPrice,
                Category = ProductCategory.Hookah,
                SubCategory = "Fruit",
                SubCategoryUk = "Фруктові",
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Tobacco", "Coconut coal" },
                IngredientsUk = new List<string> { "Тютюн", "Кокосове вугілля" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764233482/hookah_otgbcr.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Mango and Passion Fruit",
                    TobaccoFlavorUk = "Манго та Маракуя",
                    Strength = HookahStrength.Medium,
                    BowlType = "Clay",
                    BowlTypeUk = "Глиняна",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Base", "Water" },
                        { "Add-on", "Orange slice" }
                    },
                    AdditionalParamsUk = new Dictionary<string, string>
                    {
                        { "Основа", "Вода" },
                        { "Додаток", "Скибочка апельсина" }
                    }
                },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductHummusId,
                Name = SharedSeedData.ProductHummusName,
                NameUk = SharedSeedData.ProductHummusNameUk,
                Description = "Creamy chickpea dip blended with tahini, lemon juice, and garlic, served with olive oil.",
                DescriptionUk = "Кремовий соус з нуту з тахіні, лимонним соком та часником, подається з оливковою олією.",
                Price = SharedSeedData.ProductHummusPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Dips",
                SubCategoryUk = "Соуси",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Chickpeas", "Tahini", "Lemon", "Garlic", "Olive Oil" },
                IngredientsUk = new List<string> { "Нут", "Тахіні", "Лимон", "Часник", "Оливкова олія" },
                Allergens = new List<string> { "Sesame" },
                AllergensUk = new List<string> { "Кунжут" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235513/hummus_ilsuzt.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductShakshukaId,
                Name = SharedSeedData.ProductShakshukaName,
                NameUk = SharedSeedData.ProductShakshukaNameUk,
                Description = "Poached eggs in a spicy tomato and pepper sauce, served with fresh bread.",
                DescriptionUk = "Яйця-пашот у гострому томатно-перцевому соусі, подаються зі свіжим хлібом.",
                Price = SharedSeedData.ProductShakshukaPrice,
                DiscountPrice = SharedSeedData.ProductShakshukaDiscountPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Breakfast",
                SubCategoryUk = "Сніданки",
                IsVegetarian = true,
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Eggs", "Tomatoes", "Bell Peppers", "Onions", "Spices" },
                IngredientsUk = new List<string> { "Яйця", "Помідори", "Болгарський перець", "Цибуля", "Спеції" },
                Allergens = new List<string> { "Eggs" },
                AllergensUk = new List<string> { "Яйця" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235521/shakshuka_j5upk7.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductChallengeId,
                Name = SharedSeedData.ProductChallengeName,
                NameUk = SharedSeedData.ProductChallengeNameUk,
                Description = "Traditional braided Jewish bread, soft and slightly sweet, perfect for Shabbat.",
                DescriptionUk = "Традиційний плетений єврейський хліб, м'який та злегка солодкий, ідеальний для Шабату.",
                Price = SharedSeedData.ProductChallengePrice,
                Category = ProductCategory.Dish,
                SubCategory = "Bread",
                SubCategoryUk = "Хліб",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Flour", "Eggs", "Honey", "Yeast", "Oil" },
                IngredientsUk = new List<string> { "Борошно", "Яйця", "Мед", "Дріжджі", "Олія" },
                Allergens = new List<string> { "Gluten", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Яйця" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235528/bread_dvobpx.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductMatzoSoupId,
                Name = SharedSeedData.ProductMatzoSoupName,
                NameUk = SharedSeedData.ProductMatzoSoupNameUk,
                Description = "Comforting chicken broth with fluffy matzo balls, carrots, and celery.",
                DescriptionUk = "Затишний курячий бульйон з пухкими кульками маца, морквою та селерою.",
                Price = SharedSeedData.ProductMatzoSoupPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Soup",
                SubCategoryUk = "Супи",
                IsVegetarian = false,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Chicken Broth", "Matzo Meal", "Eggs", "Carrots", "Celery" },
                IngredientsUk = new List<string> { "Курячий бульйон", "Мука маца", "Яйця", "Морква", "Селера" },
                Allergens = new List<string> { "Gluten", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Яйця" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235517/matzo_ball_soup_pbf75i.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductBrisketId,
                Name = SharedSeedData.ProductBrisketName,
                NameUk = SharedSeedData.ProductBrisketNameUk,
                Description = "Slow-cooked tender beef brisket with caramelized onions and root vegetables.",
                DescriptionUk = "Повільно тушкована ніжна яловича грудинка з карамелізованою цибулею та коренеплодами.",
                Price = SharedSeedData.ProductBrisketPrice,
                DiscountPrice = SharedSeedData.ProductBrisketDiscountPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                SubCategoryUk = "Основні страви",
                IsVegetarian = false,
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Beef Brisket", "Onions", "Carrots", "Red Wine", "Herbs" },
                IngredientsUk = new List<string> { "Яловича грудинка", "Цибуля", "Морква", "Червоне вино", "Трави" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235525/beef_brisket_ylbvuf.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductSchnitzelId,
                Name = SharedSeedData.ProductSchnitzelName,
                NameUk = SharedSeedData.ProductSchnitzelNameUk,
                Description = "Crispy breaded chicken cutlet served with lemon wedge and fresh salad.",
                DescriptionUk = "Хрустка панірована куряча отбивна, подається з лимоном та свіжим салатом.",
                Price = SharedSeedData.ProductSchnitzelPrice,
                DiscountPrice = SharedSeedData.ProductSchnitzelDiscountPrice,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                SubCategoryUk = "Основні страви",
                IsVegetarian = false,
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Chicken Breast", "Breadcrumbs", "Eggs", "Flour", "Lemon" },
                IngredientsUk = new List<string> { "Куряче філе", "Панірувальні сухарі", "Яйця", "Борошно", "Лимон" },
                Allergens = new List<string> { "Gluten", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Яйця" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235532/chicken_schnitzel_vm7ecb.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductBabkaId,
                Name = SharedSeedData.ProductBabkaName,
                NameUk = SharedSeedData.ProductBabkaNameUk,
                Description = "Rich chocolate-filled twisted bread, a sweet Jewish pastry favorite.",
                DescriptionUk = "Багатий шоколадний плетений хліб, улюблена солодка єврейська випічка.",
                Price = SharedSeedData.ProductBabkaPrice,
                DiscountPrice = SharedSeedData.ProductBabkaDiscountPrice,
                Category = ProductCategory.Dessert,
                SubCategory = "Pastry",
                SubCategoryUk = "Випічка",
                IsVegetarian = true,
                IsPromotional = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Flour", "Chocolate", "Butter", "Sugar", "Eggs" },
                IngredientsUk = new List<string> { "Борошно", "Шоколад", "Масло", "Цукор", "Яйця" },
                Allergens = new List<string> { "Gluten", "Eggs", "Dairy" },
                AllergensUk = new List<string> { "Глютен", "Яйця", "Молочні продукти" },
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235536/chocolate_babka_q79pcq.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductHookahBerryId,
                Name = SharedSeedData.ProductHookahBerryName,
                NameUk = SharedSeedData.ProductHookahBerryNameUk,
                Description = "Sweet and tangy wild berry mix with raspberry, blueberry, and strawberry notes.",
                DescriptionUk = "Солодка та терпка суміш диких ягід з нотками малини, чорниці та полуниці.",
                Price = SharedSeedData.ProductHookahBerryPrice,
                Category = ProductCategory.Hookah,
                SubCategory = "Fruit",
                SubCategoryUk = "Фруктові",
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Tobacco", "Natural coal" },
                IngredientsUk = new List<string> { "Тютюн", "Натуральне вугілля" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235773/hookah_berry_hea9px.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Wild Berry Mix",
                    TobaccoFlavorUk = "Суміш диких ягід",
                    Strength = HookahStrength.Light,
                    BowlType = "Silicone",
                    BowlTypeUk = "Силіконова",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Base", "Ice Water" },
                        { "Add-on", "Berry garnish" }
                    },
                    AdditionalParamsUk = new Dictionary<string, string>
                    {
                        { "Основа", "Крижана вода" },
                        { "Додаток", "Ягідний гарнір" }
                    }
                },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductHookahMintId,
                Name = SharedSeedData.ProductHookahMintName,
                NameUk = SharedSeedData.ProductHookahMintNameUk,
                Description = "Refreshing pure mint flavor with a cool, crisp sensation.",
                DescriptionUk = "Освіжаючий чистий м'ятний смак з прохолодним, чистим відчуттям.",
                Price = SharedSeedData.ProductHookahMintPrice,
                Category = ProductCategory.Hookah,
                SubCategory = "Classic",
                SubCategoryUk = "Класичні",
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Tobacco", "Coconut coal" },
                IngredientsUk = new List<string> { "Тютюн", "Кокосове вугілля" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764235510/fresh_mint_hookah_joirpt.jpg" },
                HookahDetails = new HookahDetails
                {
                    TobaccoFlavor = "Fresh Mint",
                    TobaccoFlavorUk = "Свіжа м'ята",
                    Strength = HookahStrength.Strong,
                    BowlType = "Clay",
                    BowlTypeUk = "Глиняна",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Base", "Water with ice" },
                        { "Add-on", "Fresh mint leaves" }
                    },
                    AdditionalParamsUk = new Dictionary<string, string>
                    {
                        { "Основа", "Вода з льодом" },
                        { "Додаток", "Свіже листя м'яти" }
                    }
                },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductLemonadeId,
                Name = SharedSeedData.ProductLemonadeName,
                NameUk = SharedSeedData.ProductLemonadeNameUk,
                Description = "Freshly squeezed lemon juice with mint and a touch of honey.",
                DescriptionUk = "Свіжовичавлений лимонний сік з м'ятою та додаванням меду.",
                Price = SharedSeedData.ProductLemonadePrice,
                Category = ProductCategory.Drink,
                SubCategory = "Cold Drinks",
                SubCategoryUk = "Холодні напої",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Lemon", "Mint", "Honey", "Water", "Ice" },
                IngredientsUk = new List<string> { "Лимон", "М'ята", "Мед", "Вода", "Лід" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764236066/lemonade_with_mint_vy4wez.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductPomegranatJuiceId,
                Name = SharedSeedData.ProductPomegranatJuiceName,
                NameUk = SharedSeedData.ProductPomegranatJuiceNameUk,
                Description = "Pure pomegranate juice, rich in antioxidants and flavor.",
                DescriptionUk = "Чистий гранатовий сік, багатий на антиоксиданти та смак.",
                Price = SharedSeedData.ProductPomegranatJuicePrice,
                Category = ProductCategory.Drink,
                SubCategory = "Juices",
                SubCategoryUk = "Соки",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Pomegranate" },
                IngredientsUk = new List<string> { "Гранат" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764236061/pomegranate_juice_kvntfe.jpg" },
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = SharedSeedData.ProductMintTeaId,
                Name = SharedSeedData.ProductMintTeaName,
                NameUk = SharedSeedData.ProductMintTeaNameUk,
                Description = "Traditional Moroccan green tea infused with fresh mint leaves.",
                DescriptionUk = "Традиційний марокканський зелений чай, настояний на свіжому листі м'яти.",
                Price = SharedSeedData.ProductMintTeaPrice,
                Category = ProductCategory.Drink,
                SubCategory = "Hot Drinks",
                SubCategoryUk = "Гарячі напої",
                IsVegetarian = true,
                IsAvailable = true,
                Rating = 0,
                RatingCount = 0,
                Ingredients = new List<string> { "Green Tea", "Fresh Mint", "Sugar" },
                IngredientsUk = new List<string> { "Зелений чай", "Свіжа м'ята", "Цукор" },
                Allergens = new List<string>(),
                AllergensUk = new List<string>(),
                Photos = new List<string> { "https://res.cloudinary.com/dyeolv77j/image/upload/v1764236057/moroccan_mint_tea_aulnex.jpg" },
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("ProductSeeder: Successfully seeded {Count} products with bilingual support.", products.Count);
    }
}