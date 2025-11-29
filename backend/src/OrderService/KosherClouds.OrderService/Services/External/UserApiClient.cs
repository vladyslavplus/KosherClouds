using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserApiClient> _logger;

        public UserApiClient(HttpClient httpClient, ILogger<UserApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserInfoDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"api/users/{userId}/public",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch user {UserId}. Status: {Status}",
                        userId, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<UserInfoDto>(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId}", userId);
                return null;
            }
        }
    }
}