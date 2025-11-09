using System.Net.Http.Json;
using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public class CartApiClient : ICartApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CartApiClient> _logger;

        public CartApiClient(HttpClient httpClient, ILogger<CartApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<CartItemDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<CartResponse>(
                    "/api/cart", cancellationToken);

                if (response?.Items == null)
                {
                    _logger.LogWarning("Empty cart response for user {UserId}", userId);
                    return new List<CartItemDto>();
                }

                _logger.LogInformation(
                    "Retrieved cart for user {UserId} with {Count} items",
                    userId,
                    response.Items.Count);

                return response.Items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "HTTP request failed while getting cart for user {UserId}. Status: {StatusCode}",
                    userId,
                    ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync("/api/cart", cancellationToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully cleared cart for user {UserId}", userId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "HTTP request failed while clearing cart for user {UserId}. Status: {StatusCode}",
                    userId,
                    ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear cart for user {UserId}", userId);
                throw;
            }
        }

        private sealed class CartResponse
        {
            public Guid UserId { get; set; }
            public List<CartItemDto> Items { get; set; } = new();
        }
    }
}