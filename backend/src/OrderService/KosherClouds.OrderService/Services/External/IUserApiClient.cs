using KosherClouds.OrderService.DTOs.External;

namespace KosherClouds.OrderService.Services.External
{
    public interface IUserApiClient
    {
        Task<UserInfoDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
