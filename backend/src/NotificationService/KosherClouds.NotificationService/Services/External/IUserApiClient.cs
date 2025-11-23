using KosherClouds.NotificationService.DTOs.User;

namespace KosherClouds.NotificationService.Services.External
{
    public interface IUserApiClient
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
