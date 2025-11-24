using KosherClouds.UserService.DTOs.Auth;
using KosherClouds.UserService.DTOs.Token;

namespace KosherClouds.UserService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, TokenResponse? Tokens)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string? Error, TokenResponse? Tokens)> LoginAsync(LoginRequest request);
        Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
