using KosherClouds.Contracts.Users;
using KosherClouds.UserService.DTOs.Auth;
using KosherClouds.UserService.DTOs.Token;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace KosherClouds.UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<(bool Success, string? Error, TokenResponse? Tokens)> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return (false, "Email already registered", null);

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
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
    }
}