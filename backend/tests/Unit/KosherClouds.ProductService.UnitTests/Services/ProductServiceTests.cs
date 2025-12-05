using AutoMapper;
using FluentAssertions;
using KosherClouds.ProductService.Data;
using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.ProductService.Parameters;
using KosherClouds.ProductService.UnitTests.Helpers;
using KosherClouds.ServiceDefaults.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using ProductServiceClass = KosherClouds.ProductService.Services.ProductService;

namespace KosherClouds.ProductService.UnitTests.Services
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<ProductServiceClass>> _loggerMock;
        private readonly Mock<ISortHelperFactory> _sortHelperFactoryMock;
        private readonly Mock<ISortHelper<Product>> _sortHelperMock;
        private readonly ProductServiceClass _productService;
        private bool _disposed;

        public ProductServiceTests()
        {
            _dbContext = MockDbContextFactory.Create();
            _mapper = AutoMapperFactory.Create();
            _loggerMock = new Mock<ILogger<ProductServiceClass>>();

            _sortHelperMock = new Mock<ISortHelper<Product>>();
            _sortHelperMock
                .Setup(x => x.ApplySort(It.IsAny<IQueryable<Product>>(), It.IsAny<string>()))
                .Returns<IQueryable<Product>, string>((query, orderBy) => query);

            _sortHelperFactoryMock = new Mock<ISortHelperFactory>();
            _sortHelperFactoryMock
                .Setup(x => x.Create<Product>())
                .Returns(_sortHelperMock.Object);

            _productService = new ProductServiceClass(
                _dbContext,
                _mapper,
                _sortHelperFactoryMock.Object,
                _loggerMock.Object,
                isInMemory: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region GetProductsAsync Tests

        [Fact]
        public async Task GetProductsAsync_WhenNoFilters_ReturnsAllProducts()
        {
            // Arrange
            var products = ProductTestData.CreateProductList(5);
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetProductsAsync_WithNameFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.Name = "Kugel Special";

            var product2 = ProductTestData.CreateValidProduct();
            product2.Name = "Falafel Deluxe";

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                Name = "Kugel",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Name.Should().Be("Kugel Special");
        }

        [Fact]
        public async Task GetProductsAsync_WithNameUkFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.Name = "Kugel";
            product1.NameUk = "Кугель";

            var product2 = ProductTestData.CreateValidProduct();
            product2.Name = "Falafel";
            product2.NameUk = "Фалафель";

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                NameUk = "Кугель",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].NameUk.Should().Be("Кугель");
        }

        [Fact]
        public async Task GetProductsAsync_WithSubCategoryFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.SubCategory = "Main Course";

            var product2 = ProductTestData.CreateValidProduct();
            product2.SubCategory = "Appetizer";

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                SubCategory = "Main Course",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].SubCategory.Should().Be("Main Course");
        }

        [Fact]
        public async Task GetProductsAsync_WithSubCategoryUkFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.SubCategory = "Main Course";
            product1.SubCategoryUk = "Основна страва";

            var product2 = ProductTestData.CreateValidProduct();
            product2.SubCategory = "Appetizer";
            product2.SubCategoryUk = "Закуска";

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                SubCategoryUk = "Основна страва",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].SubCategoryUk.Should().Be("Основна страва");
        }

        [Fact]
        public async Task GetProductsAsync_WithIsPromotionalFilter_ReturnsOnlyPromotionalProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.IsPromotional = true;
            product1.DiscountPrice = 50m;

            var product2 = ProductTestData.CreateValidProduct();
            product2.IsPromotional = false;

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                IsPromotional = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].IsPromotional.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductsAsync_WithPriceRange_ReturnsProductsInRange()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.Price = 100m;

            var product2 = ProductTestData.CreateValidProduct();
            product2.Price = 200m;

            var product3 = ProductTestData.CreateValidProduct();
            product3.Price = 300m;

            await _dbContext.Products.AddRangeAsync(product1, product2, product3);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                MinPrice = 150m,
                MaxPrice = 250m,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Price.Should().Be(200m);
        }

        [Fact]
        public async Task GetProductsAsync_WithIsAvailableFilter_ReturnsOnlyAvailableProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.IsAvailable = true;

            var product2 = ProductTestData.CreateValidProduct();
            product2.IsAvailable = false;

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                IsAvailable = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].IsAvailable.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductsAsync_WithIsVegetarianFilter_ReturnsOnlyVegetarianProducts()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.IsVegetarian = true;

            var product2 = ProductTestData.CreateValidProduct();
            product2.IsVegetarian = false;

            await _dbContext.Products.AddRangeAsync(product1, product2);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                IsVegetarian = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].IsVegetarian.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var products = ProductTestData.CreateProductList(15);
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                PageNumber = 2,
                PageSize = 5
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result.TotalCount.Should().Be(15);
            result.CurrentPage.Should().Be(2);
            result.TotalPages.Should().Be(3);
            result.HasNext.Should().BeTrue();
            result.HasPrevious.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductsAsync_WithCategoryFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var dishProduct = ProductTestData.CreateValidProduct();
            dishProduct.Category = ProductCategory.Dish;

            var hookahProduct = ProductTestData.CreateHookahProduct();

            await _dbContext.Products.AddRangeAsync(dishProduct, hookahProduct);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                Category = "Hookah",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Category.Should().Be(ProductCategory.Hookah);
        }

        [Fact]
        public async Task GetProductsAsync_WithDateRangeFrom_ReturnsRecentProducts()
        {
            // Arrange
            var oldProduct = ProductTestData.CreateValidProduct();
            oldProduct.CreatedAt = DateTime.UtcNow.AddDays(-30);

            var recentProduct = ProductTestData.CreateValidProduct();
            recentProduct.CreatedAt = DateTime.UtcNow.AddDays(-5);

            await _dbContext.Products.AddRangeAsync(oldProduct, recentProduct);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                CreatedAtFrom = DateTime.UtcNow.AddDays(-10),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetProductsAsync_WithDateRangeTo_ReturnsOldProducts()
        {
            // Arrange
            var oldProduct = ProductTestData.CreateValidProduct();
            oldProduct.CreatedAt = DateTime.UtcNow.AddDays(-30);

            var recentProduct = ProductTestData.CreateValidProduct();
            recentProduct.CreatedAt = DateTime.UtcNow.AddDays(-5);

            await _dbContext.Products.AddRangeAsync(oldProduct, recentProduct);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                CreatedAtTo = DateTime.UtcNow.AddDays(-10),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetProductsAsync_WithMultipleFilters_ReturnsMatchingProducts()
        {
            // Arrange
            var matchingProduct = ProductTestData.CreateValidProduct();
            matchingProduct.IsVegetarian = true;
            matchingProduct.IsAvailable = true;
            matchingProduct.Price = 150m;

            var nonMatchingProduct = ProductTestData.CreateValidProduct();
            nonMatchingProduct.IsVegetarian = false;

            await _dbContext.Products.AddRangeAsync(matchingProduct, nonMatchingProduct);
            await _dbContext.SaveChangesAsync();

            var parameters = new ProductParameters
            {
                IsVegetarian = true,
                IsAvailable = true,
                MinPrice = 100m,
                MaxPrice = 200m,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].IsVegetarian.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductsAsync_WhenNoProductsMatch_ReturnsEmptyList()
        {
            // Arrange
            var parameters = new ProductParameters
            {
                Name = "NonExistentProduct",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
            result.TotalCount.Should().Be(0);
        }

        #endregion

        #region GetSubCategoriesAsync Tests

        [Fact]
        public async Task GetSubCategoriesAsync_ForDishCategory_ReturnsDistinctSubCategories()
        {
            // Arrange
            var product1 = ProductTestData.CreateValidProduct();
            product1.Category = ProductCategory.Dish;
            product1.SubCategory = "Main Course";
            product1.SubCategoryUk = "Основна страва";

            var product2 = ProductTestData.CreateValidProduct();
            product2.Category = ProductCategory.Dish;
            product2.SubCategory = "Main Course";
            product2.SubCategoryUk = "Основна страва";

            var product3 = ProductTestData.CreateValidProduct();
            product3.Category = ProductCategory.Dish;
            product3.SubCategory = "Appetizer";
            product3.SubCategoryUk = "Закуска";

            await _dbContext.Products.AddRangeAsync(product1, product2, product3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetSubCategoriesAsync(ProductCategory.Dish);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Value == "Main Course" && s.LabelUk == "Основна страва");
            result.Should().Contain(s => s.Value == "Appetizer" && s.LabelUk == "Закуска");
        }

        [Fact]
        public async Task GetSubCategoriesAsync_ForCategoryWithNoSubCategories_ReturnsEmptyList()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.Category = ProductCategory.Dessert;
            product.SubCategory = null;

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetSubCategoriesAsync(ProductCategory.Dessert);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSubCategoriesAsync_ReturnsOnlyForSpecifiedCategory()
        {
            // Arrange
            var dishProduct = ProductTestData.CreateValidProduct();
            dishProduct.Category = ProductCategory.Dish;
            dishProduct.SubCategory = "Main Course";

            var dessertProduct = ProductTestData.CreateValidProduct();
            dessertProduct.Category = ProductCategory.Dessert;
            dessertProduct.SubCategory = "Cake";

            await _dbContext.Products.AddRangeAsync(dishProduct, dessertProduct);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetSubCategoriesAsync(ProductCategory.Dish);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Value.Should().Be("Main Course");
        }

        #endregion

        #region GetProductByIdAsync Tests

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = ProductTestData.CreateKugelProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductByIdAsync(product.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(product.Id);
            result.Name.Should().Be(product.Name);
            result.Price.Should().Be(product.Price);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _productService.GetProductByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateProductAsync Tests

        [Fact]
        public async Task CreateProductAsync_WithValidRequest_CreatesProduct()
        {
            // Arrange
            var createRequest = ProductTestData.CreateValidProductCreateRequest();

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createRequest.Name);
            result.Price.Should().Be(createRequest.Price);
            result.Category.Should().Be(createRequest.Category);

            var productInDb = await _dbContext.Products.FindAsync(result.Id);
            productInDb.Should().NotBeNull();
            productInDb!.Name.Should().Be(createRequest.Name);
        }

        [Fact]
        public async Task CreateProductAsync_WithUkrainianFields_CreatesProductWithTranslations()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Kugel",
                NameUk = "Кугель",
                Description = "Traditional Jewish noodle casserole",
                DescriptionUk = "Традиційна єврейська запіканка з локшини",
                Price = 120m,
                Category = ProductCategory.Dish,
                SubCategory = "Main Course",
                SubCategoryUk = "Основна страва"
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.NameUk.Should().Be("Кугель");
            result.DescriptionUk.Should().Be("Традиційна єврейська запіканка з локшини");
            result.SubCategoryUk.Should().Be("Основна страва");
        }

        [Fact]
        public async Task CreateProductAsync_WithDiscountPrice_CreatesPromotionalProduct()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Special Offer Falafel",
                Description = "Limited time offer",
                Price = 100m,
                DiscountPrice = 75m,
                IsPromotional = true,
                Category = ProductCategory.Set
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Price.Should().Be(100m);
            result.DiscountPrice.Should().Be(75m);
            result.IsPromotional.Should().BeTrue();
        }

        [Fact]
        public async Task CreateProductAsync_WithIngredientsAndAllergens_CreatesProduct()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Kugel",
                Description = "Noodle casserole",
                Price = 120m,
                Category = ProductCategory.Dish,
                Ingredients = new List<string> { "Noodles", "Eggs", "Cottage Cheese" },
                IngredientsUk = new List<string> { "Локшина", "Яйця", "Сир" },
                Allergens = new List<string> { "Gluten", "Dairy", "Eggs" },
                AllergensUk = new List<string> { "Глютен", "Молочні продукти", "Яйця" }
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Ingredients.Should().HaveCount(3);
            result.IngredientsUk.Should().HaveCount(3);
            result.Allergens.Should().HaveCount(3);
            result.AllergensUk.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateProductAsync_WithMultiplePhotos_CreatesProduct()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Deluxe Falafel",
                Description = "Best falafel in town",
                Price = 150m,
                Category = ProductCategory.Set,
                Photos = new List<string> { "photo1.jpg", "photo2.jpg", "photo3.jpg" }
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Photos.Should().HaveCount(3);
            result.Photos.Should().Contain("photo1.jpg");
        }

        [Fact]
        public async Task CreateProductAsync_ShouldLogInformation()
        {
            // Arrange
            var createRequest = ProductTestData.CreateValidProductCreateRequest();

            // Act
            await _productService.CreateProductAsync(createRequest);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithMinimalData_CreatesProduct()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Minimal Product",
                Description = "Simple product",
                Price = 50m,
                Category = ProductCategory.Dish
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateProductAsync_WithHookahDetails_CreatesProductWithDetails()
        {
            // Arrange
            var createRequest = new ProductCreateRequest
            {
                Name = "Premium Hookah",
                Description = "Best hookah experience",
                Price = 500m,
                Category = ProductCategory.Hookah,
                HookahDetails = new HookahDetailsDto
                {
                    TobaccoFlavor = "Mint",
                    TobaccoFlavorUk = "М'ята",
                    Strength = HookahStrength.Strong,
                    BowlType = "Phunnel",
                    BowlTypeUk = "Фаннел",
                    AdditionalParams = new Dictionary<string, string>
                    {
                        { "Duration", "90 minutes" }
                    },
                    AdditionalParamsUk = new Dictionary<string, string>
                    {
                        { "Duration", "90 хвилин" }
                    }
                }
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.HookahDetails.Should().NotBeNull();
            result.HookahDetails!.TobaccoFlavor.Should().Be("Mint");
            result.HookahDetails.TobaccoFlavorUk.Should().Be("М'ята");
            result.HookahDetails.Strength.Should().Be(HookahStrength.Strong);
            result.HookahDetails.AdditionalParams.Should().ContainKey("Duration");
        }

        #endregion

        #region UpdateProductAsync Tests

        [Fact]
        public async Task UpdateProductAsync_WithValidRequest_UpdatesProduct()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Name = "Updated Product Name",
                Price = 999.99m,
                IsAvailable = false
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updatedProduct = await _dbContext.Products.FindAsync(product.Id);
            updatedProduct.Should().NotBeNull();
            updatedProduct!.Name.Should().Be("Updated Product Name");
            updatedProduct.Price.Should().Be(999.99m);
            updatedProduct.IsAvailable.Should().BeFalse();
            updatedProduct.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateProductAsync_WithUkrainianFields_UpdatesTranslations()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                NameUk = "Оновлена назва",
                DescriptionUk = "Оновлений опис",
                SubCategoryUk = "Оновлена підкатегорія"
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.NameUk.Should().Be("Оновлена назва");
            updated.DescriptionUk.Should().Be("Оновлений опис");
            updated.SubCategoryUk.Should().Be("Оновлена підкатегорія");
        }

        [Fact]
        public async Task UpdateProductAsync_WithDiscountPrice_UpdatesPromotionalStatus()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.IsPromotional = false;
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                DiscountPrice = 80m,
                IsPromotional = true
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.DiscountPrice.Should().Be(80m);
            updated.IsPromotional.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProductAsync_WithPhotos_UpdatesPhotosList()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.Photos = new List<string> { "old1.jpg" };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Photos = new List<string> { "new1.jpg", "new2.jpg", "new3.jpg" }
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.Photos.Should().HaveCount(3);
            updated.Photos.Should().Contain("new1.jpg");
        }

        [Fact]
        public async Task UpdateProductAsync_WithIngredientsAndAllergens_UpdatesLists()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Ingredients = new List<string> { "New Ingredient 1", "New Ingredient 2" },
                IngredientsUk = new List<string> { "Новий інгредієнт 1", "Новий інгредієнт 2" },
                Allergens = new List<string> { "Nuts" },
                AllergensUk = new List<string> { "Горіхи" }
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.Ingredients.Should().HaveCount(2);
            updated.IngredientsUk.Should().HaveCount(2);
            updated.Allergens.Should().HaveCount(1);
            updated.AllergensUk.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateRequest = ProductTestData.CreateValidProductUpdateRequest();

            // Act
            Func<Task> act = async () => await _productService.UpdateProductAsync(nonExistentId, updateRequest);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Product with ID '{nonExistentId}' not found.");
        }

        [Fact]
        public async Task UpdateProductAsync_WithPartialUpdate_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            var originalName = product.Name;
            var originalPrice = product.Price;

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                IsAvailable = false
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updatedProduct = await _dbContext.Products.FindAsync(product.Id);
            updatedProduct.Should().NotBeNull();
            updatedProduct!.Name.Should().Be(originalName);
            updatedProduct.Price.Should().Be(originalPrice);
            updatedProduct.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProductAsync_UpdateOnlyPrice_KeepsOtherFieldsUnchanged()
        {
            // Arrange
            var product = ProductTestData.CreateKugelProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Price = 150m
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.Price.Should().Be(150m);
            updated.Name.Should().Be(product.Name);
            updated.Category.Should().Be(product.Category);
        }

        [Fact]
        public async Task UpdateProductAsync_UpdateRatingAndCount_UpdatesCorrectly()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.Rating = 3.0;
            product.RatingCount = 5;

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Rating = 4.5,
                RatingCount = 20
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.Rating.Should().Be(4.5);
            updated.RatingCount.Should().Be(20);
        }

        [Fact]
        public async Task UpdateProductAsync_AddHookahDetailsToExistingProduct_AddsDetails()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.Category = ProductCategory.Hookah;
            product.HookahDetails = null;

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                HookahDetails = new HookahDetailsDto
                {
                    TobaccoFlavor = "Apple",
                    Strength = HookahStrength.Light,
                    BowlType = "Egyptian"
                }
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.HookahDetails.Should().NotBeNull();
            updated.HookahDetails!.TobaccoFlavor.Should().Be("Apple");
            updated.HookahDetails.Strength.Should().Be(HookahStrength.Light);
        }

        [Fact]
        public async Task UpdateProductAsync_ModifyExistingHookahDetails_UpdatesDetails()
        {
            // Arrange
            var product = ProductTestData.CreateHookahProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                HookahDetails = new HookahDetailsDto
                {
                    TobaccoFlavor = "Watermelon",
                    TobaccoFlavorUk = "Кавун",
                    Strength = HookahStrength.Medium,
                    BowlType = "Silicone",
                    BowlTypeUk = "Силіконова"
                }
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.HookahDetails.Should().NotBeNull();
            updated.HookahDetails!.TobaccoFlavor.Should().Be("Watermelon");
            updated.HookahDetails.TobaccoFlavorUk.Should().Be("Кавун");
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesUpdatedAtTimestamp()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            product.UpdatedAt = DateTime.UtcNow.AddDays(-1);

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var updateRequest = new ProductUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            await _productService.UpdateProductAsync(product.Id, updateRequest);

            // Assert
            var updated = await _dbContext.Products.FindAsync(product.Id);
            updated!.UpdatedAt.Should().NotBeNull();
            updated.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region DeleteProductAsync Tests

        [Fact]
        public async Task DeleteProductAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            await _productService.DeleteProductAsync(product.Id);

            // Assert
            var deletedProduct = await _dbContext.Products.FindAsync(product.Id);
            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task DeleteProductAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _productService.DeleteProductAsync(nonExistentId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Product with ID '{nonExistentId}' not found.");
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldLogInformation()
        {
            // Arrange
            var product = ProductTestData.CreateValidProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            await _productService.DeleteProductAsync(product.Id);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("deleted successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductWithHookahDetails_DeletesSuccessfully()
        {
            // Arrange
            var product = ProductTestData.CreateHookahProduct();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            await _productService.DeleteProductAsync(product.Id);

            // Assert
            var deleted = await _dbContext.Products.FindAsync(product.Id);
            deleted.Should().BeNull();
        }

        #endregion
    }
}