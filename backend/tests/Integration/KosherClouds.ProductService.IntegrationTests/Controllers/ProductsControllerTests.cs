using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using KosherClouds.ProductService.DTOs.Products;
using KosherClouds.ProductService.Entities.Enums;
using KosherClouds.ProductService.IntegrationTests.Infrastructure;
using KosherClouds.ProductService.DTOs.Hookah;
using KosherClouds.ProductService.DTOs.Photo;

namespace KosherClouds.ProductService.IntegrationTests.Controllers
{
    public class ProductsControllerTests : IClassFixture<ProductServiceWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public ProductsControllerTests(ProductServiceWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProducts_WithoutAuthentication_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnPagedResults()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            await CreateProduct("Test Product 1", ProductCategory.Dish);
            await CreateProduct("Test Product 2", ProductCategory.Drink);
            await CreateProduct("Test Product 3", ProductCategory.Hookah);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/products?pageSize=2");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>(JsonOptions);
            products.Should().NotBeNull();
            products!.Should().HaveCountLessOrEqualTo(2);

            response.Headers.Should().ContainKey("X-Pagination");
        }

        [Fact]
        public async Task GetProducts_FilterByCategory_ShouldReturnFilteredResults()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            await CreateProduct("Dish Item", ProductCategory.Dish);
            await CreateProduct("Drink Item", ProductCategory.Drink);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/products?category=Dish");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>(JsonOptions);
            products.Should().NotBeNull();
            products!.Should().OnlyContain(p => p.Category == ProductCategory.Dish);
        }

        [Fact]
        public async Task GetProducts_FilterByPriceRange_ShouldReturnFilteredResults()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            await CreateProduct("Cheap Item", ProductCategory.Dish, price: 50m);
            await CreateProduct("Expensive Item", ProductCategory.Dish, price: 500m);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/products?minPrice=100&maxPrice=600");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>(JsonOptions);
            products.Should().NotBeNull();
            products!.Should().OnlyContain(p => p.Price >= 100m && p.Price <= 600m);
        }

        [Fact]
        public async Task GetProducts_SearchByName_ShouldReturnMatchingProducts()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            await CreateProduct("Caesar Salad", ProductCategory.Dish);
            await CreateProduct("Greek Salad", ProductCategory.Dish);
            await CreateProduct("Pasta Carbonara", ProductCategory.Dish);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/products?name=Salad");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>(JsonOptions);
            products.Should().NotBeNull();
            products!.Should().HaveCountGreaterOrEqualTo(2);
            products!.Should().OnlyContain(p => p.Name.Contains("Salad", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Test Product", ProductCategory.Dish);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync($"/api/products/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
            product.Should().NotBeNull();
            product!.Id.Should().Be(productId);
        }

        [Fact]
        public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/products/{nonExistentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSubCategories_ShouldReturnDistinctSubCategories()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            await CreateProduct("Salad 1", ProductCategory.Dish, subCategory: "Salads");
            await CreateProduct("Salad 2", ProductCategory.Dish, subCategory: "Salads");
            await CreateProduct("Soup 1", ProductCategory.Dish, subCategory: "Soups");

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/products/subcategories?category=Dish");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var subCategories = await response.Content.ReadFromJsonAsync<List<dynamic>>(JsonOptions);
            subCategories.Should().NotBeNull();
            subCategories!.Should().HaveCountGreaterOrEqualTo(2);
        }

        [Fact]
        public async Task CreateProduct_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var dto = CreateProductDto("Test Product", ProductCategory.Dish);

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateProduct_AsRegularUser_ShouldReturnForbidden()
        {
            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId, new[] { "User" });

            var dto = CreateProductDto("Test Product", ProductCategory.Dish);

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateProduct_AsAdmin_ShouldReturnCreated()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var dto = CreateProductDto("New Product", ProductCategory.Dish);

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
            product.Should().NotBeNull();
            product!.Name.Should().Be("New Product");
            product.Category.Should().Be(ProductCategory.Dish);
        }

        [Fact]
        public async Task CreateProduct_AsManager_ShouldReturnCreated()
        {
            var managerId = Guid.NewGuid();
            _client.AddAuthorizationHeader(managerId, new[] { "Manager" });

            var dto = CreateProductDto("Manager Product", ProductCategory.Drink);

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateProduct_WithHookahDetails_ShouldIncludeDetails()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var dto = CreateProductDto("Hookah Product", ProductCategory.Hookah);
            dto.HookahDetails = new HookahDetailsDto
            {
                TobaccoFlavor = "Mint",
                TobaccoFlavorUk = "М'ята",
                Strength = HookahStrength.Medium,
                BowlType = "Phunnel",
                BowlTypeUk = "Фаннел"
            };

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
            product.Should().NotBeNull();
            product!.HookahDetails.Should().NotBeNull();
            product.HookahDetails!.TobaccoFlavor.Should().Be("Mint");
            product.HookahDetails.Strength.Should().Be(HookahStrength.Medium);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var dto = new ProductCreateRequest
            {
                Name = "",
                Description = "Test",
                Price = -10m,
                Category = ProductCategory.Dish
            };

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateProduct_AsAdmin_ShouldUpdateSuccessfully()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Original Product", ProductCategory.Dish);

            var updateDto = new ProductUpdateRequest
            {
                Name = "Updated Product",
                Price = 199.99m,
                IsAvailable = false
            };

            var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _client.DefaultRequestHeaders.Authorization = null;
            var getResponse = await _client.GetAsync($"/api/products/{productId}");
            var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
            product!.Name.Should().Be("Updated Product");
            product.Price.Should().Be(199.99m);
            product.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var updateDto = new ProductUpdateRequest { Name = "Updated" };
            var nonExistentId = Guid.NewGuid();

            var response = await _client.PutAsJsonAsync($"/api/products/{nonExistentId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateProduct_AsRegularUser_ShouldReturnForbidden()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            var productId = await CreateProduct("Product", ProductCategory.Dish);

            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId, new[] { "User" });

            var updateDto = new ProductUpdateRequest { Name = "Hacked" };

            var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteProduct_AsAdmin_ShouldDeleteSuccessfully()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Product to Delete", ProductCategory.Dish);

            var response = await _client.DeleteAsync($"/api/products/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _client.DefaultRequestHeaders.Authorization = null;
            var getResponse = await _client.GetAsync($"/api/products/{productId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var nonExistentId = Guid.NewGuid();

            var response = await _client.DeleteAsync($"/api/products/{nonExistentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_AsRegularUser_ShouldReturnForbidden()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });
            var productId = await CreateProduct("Product", ProductCategory.Dish);

            var userId = Guid.NewGuid();
            _client.AddAuthorizationHeader(userId, new[] { "User" });

            var response = await _client.DeleteAsync($"/api/products/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetProductPhotos_WithValidId_ShouldReturnPhotos()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Product with Photos", ProductCategory.Dish);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync($"/api/products/{productId}/photos");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProductPhotos_WithInvalidId_ShouldReturnNotFound()
        {
            var nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/products/{nonExistentId}/photos");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddProductPhotos_AsAdmin_ShouldAddPhotosFromUrls()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Product", ProductCategory.Dish);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent("https://example.com/photo1.jpg"), "Urls");
            content.Add(new StringContent("https://example.com/photo2.jpg"), "Urls");

            var response = await _client.PostAsync($"/api/products/{productId}/photos", content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RemoveProductPhoto_AsAdmin_WithExistingPhoto_ShouldRemovePhoto()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Product with Photo", ProductCategory.Dish);

            var getResponse = await _client.GetAsync($"/api/products/{productId}");
            var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);

            if (product!.Photos.Count > 0)
            {
                var photoUrl = product.Photos[0];
                var response = await _client.DeleteAsync($"/api/products/{productId}/photos?photoUrl={Uri.EscapeDataString(photoUrl)}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task RemoveProductPhoto_AsAdmin_WithNonExistingPhoto_ShouldReturnNotFound()
        {
            var adminId = Guid.NewGuid();
            _client.AddAuthorizationHeader(adminId, new[] { "Admin" });

            var productId = await CreateProduct("Product", ProductCategory.Dish);
            var photoUrl = "https://test.cloudinary.com/nonexistent.jpg";

            var response = await _client.DeleteAsync($"/api/products/{productId}/photos?photoUrl={Uri.EscapeDataString(photoUrl)}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static int _productCounter = 0;

        private async Task<Guid> CreateProduct(
            string name,
            ProductCategory category,
            decimal price = 99.99m,
            string? subCategory = null)
        {
            var counter = Interlocked.Increment(ref _productCounter);
            var dto = CreateProductDto($"{name} {counter}", category, price, subCategory);

            var response = await _client.PostAsJsonAsync("/api/products", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"CreateProduct failed. Status: {response.StatusCode}. Body: {errorContent}");
            }

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);
            return product!.Id;
        }

        private static ProductCreateRequest CreateProductDto(
            string name,
            ProductCategory category,
            decimal price = 99.99m,
            string? subCategory = null)
        {
            return new ProductCreateRequest
            {
                Name = name,
                NameUk = $"{name} УК",
                Description = $"Description for {name}",
                DescriptionUk = $"Опис для {name}",
                Price = price,
                Category = category,
                SubCategory = subCategory,
                IsVegetarian = false,
                IsPromotional = false,
                IsAvailable = true,
                Ingredients = new List<string> { "Ingredient 1", "Ingredient 2" },
                IngredientsUk = new List<string> { "Інгредієнт 1", "Інгредієнт 2" },
                Allergens = new List<string> { "Allergen 1" },
                AllergensUk = new List<string> { "Алерген 1" },
                Photos = new List<string> { "https://test.com/photo.jpg" }
            };
        }
    }
}