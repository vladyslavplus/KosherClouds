using KosherClouds.Contracts.Users;
using KosherClouds.UserService.DTOs.Auth;
using KosherClouds.UserService.DTOs.Token;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace KosherClouds.UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IPublishEndpoint publishEndpoint,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error, TokenResponse? Tokens)> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return (false, "Email already registered", null);

            var existingPhone = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (existingPhone != null)
                return (false, "Phone number already registered", null);

            var nameParts = request.UserName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : null;
            var lastName = nameParts.Length > 1 ? nameParts[1] : null;

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = false,
                FirstName = firstName,
                LastName = lastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            await _userManager.AddToRoleAsync(user, "User");

            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            });

            var tokens = await _tokenService.GenerateTokensAsync(user);
            return (true, null, tokens);
        }

        public async Task<(bool Success, string? Error, TokenResponse? Tokens)> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return (false, "Invalid credentials", null);

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return (false, "Invalid credentials", null);

            var tokens = await _tokenService.GenerateTokensAsync(user);
            return (true, null, tokens);
        }

        public async Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return (true, null);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            await _publishEndpoint.Publish(new PasswordResetRequestedEvent
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName ?? "User",
                ResetToken = encodedToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RequestedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Password reset requested for user {UserId}", user.Id);

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return (false, "Invalid request");

            string decodedToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return (false, "Invalid token");
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                return (false, errors);
            }

            _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);

            return (true, null);
        }
    }
}