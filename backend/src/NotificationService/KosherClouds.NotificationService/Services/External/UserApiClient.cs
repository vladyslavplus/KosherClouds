using KosherClouds.NotificationService.DTOs.User;

namespace KosherClouds.NotificationService.Services.External
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

        public async Task<UserDto?> GetUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching user {UserId} from UserService", userId);

                var response = await _httpClient.GetAsync(
                    $"/api/users/{userId}/public",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch user {UserId}. Status: {Status}",
                        userId, response.StatusCode);
                    return null;
                }

                var user = await response.Content.ReadFromJsonAsync<UserDto>(
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully fetched user {UserId}", userId);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId} from UserService", userId);
                return null;
            }
        }
    }
}
