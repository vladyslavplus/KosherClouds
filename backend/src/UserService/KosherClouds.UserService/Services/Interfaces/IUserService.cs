using KosherClouds.ServiceDefaults.Helpers;
using KosherClouds.UserService.DTOs.User;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Parameters;

namespace KosherClouds.UserService.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedList<ApplicationUser>> GetUsersAsync(UserParameters parameters, CancellationToken cancellationToken = default);
        Task<UserProfileDto?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserPublicDto?> GetUserPublicInfoAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
