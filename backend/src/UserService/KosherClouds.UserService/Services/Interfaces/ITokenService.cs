using KosherClouds.UserService.DTOs.Token;
using KosherClouds.UserService.Entities;

namespace KosherClouds.UserService.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(ApplicationUser user);
        Task<TokenResponse?> RefreshTokenAsync(Guid userId);
        Task<bool> RevokeRefreshTokenAsync(ApplicationUser user);
    }
}
