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
        public async Task GetProductsAsync_WithDateRangeFilter_ReturnsProductsInRange()
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

            // Verify in database
            var productInDb = await _dbContext.Products.FindAsync(result.Id);
            productInDb.Should().NotBeNull();
            productInDb!.Name.Should().Be(createRequest.Name);
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
                Price = 500m,
                Category = ProductCategory.Hookah,
                HookahDetails = new HookahDetailsDto
                {
                    TobaccoFlavor = "Mint",
                    Strength = HookahStrength.Strong,
                    BowlType = "Phunnel"
                }
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.HookahDetails.Should().NotBeNull();
            result.HookahDetails!.TobaccoFlavor.Should().Be("Mint");
            result.HookahDetails.Strength.Should().Be(HookahStrength.Strong);
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
            updatedProduct!.Name.Should().Be(originalName); // Unchanged
            updatedProduct.Price.Should().Be(originalPrice); // Unchanged
            updatedProduct.IsAvailable.Should().BeFalse(); // Updated
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