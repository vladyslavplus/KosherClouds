using KosherClouds.ReviewService.DTOs.External;

namespace KosherClouds.ReviewService.Services.External
{
    public interface IUserApiClient
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, string>> GetUserNamesByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
