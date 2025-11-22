using KosherClouds.ReviewService.DTOs.External;
using System.Net.Http.Json;

namespace KosherClouds.ReviewService.Services.External
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderApiClient> _logger;

        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching order {OrderId} from OrderService", orderId);

                var response = await _httpClient.GetAsync(
                    $"/api/orders/{orderId}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch order {OrderId}. Status: {Status}",
                        orderId, response.StatusCode);
                    return null;
                }

                var order = await response.Content.ReadFromJsonAsync<OrderDto>(
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully fetched order {OrderId}", orderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId} from OrderService", orderId);
                return null;
            }
        }

        public async Task<List<OrderDto>> GetPaidOrdersForUserAsync(
            Guid userId,
            int daysBack,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var minDate = DateTimeOffset.UtcNow.AddDays(-daysBack);

                _logger.LogInformation(
                    "Fetching paid orders for user {UserId} from {MinDate}",
                    userId, minDate);

                var encodedDate = Uri.EscapeDataString(minDate.ToString("O"));

                var response = await _httpClient.GetAsync(
                    $"/api/orders?userId={userId}&status=Paid&minOrderDate={encodedDate}&pageSize=50",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Failed to fetch orders for user {UserId}. Status: {Status}, Error: {Error}",
                        userId, response.StatusCode, errorContent);
                    return new List<OrderDto>();
                }

                var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Successfully fetched {Count} paid orders for user {UserId}",
                    orders?.Count ?? 0, userId);

                return orders ?? new List<OrderDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching paid orders for user {UserId} from OrderService",
                    userId);
                return new List<OrderDto>();
            }
        }
    }
}