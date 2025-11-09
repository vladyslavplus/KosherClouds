using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProductInfoDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch product {ProductId}. Status: {StatusCode}", productId, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ProductInfoDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", productId);
                return null;
            }
        }
    }
}
