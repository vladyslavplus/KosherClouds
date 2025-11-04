using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KosherClouds.UserService.Config;
using KosherClouds.UserService.Data;
using KosherClouds.UserService.DTOs.Token;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KosherClouds.UserService.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserDbContext _db;
        private readonly JwtSettings _jwtSettings;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            UserDbContext db,
            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _db = db;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<TokenResponse> GenerateTokensAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            var oldTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.Revoked == null)
                .ToListAsync();

            foreach (var t in oldTokens)
                t.Revoked = DateTime.UtcNow;

            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            await _db.RefreshTokens.AddAsync(refreshEntity);
            await _db.SaveChangesAsync();

            return new TokenResponse(accessToken, refreshToken);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<TokenResponse?> RefreshTokenAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            var validToken = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
                .OrderByDescending(rt => rt.Created)
                .FirstOrDefaultAsync();

            if (validToken == null) return null;

            validToken.Revoked = DateTime.UtcNow;

            var newTokens = await GenerateTokensAsync(user);
            await _db.SaveChangesAsync();

            return newTokens;
        }

        public async Task<bool> RevokeRefreshTokenAsync(ApplicationUser user)
        {
            var tokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
                .ToListAsync();

            if (!tokens.Any())
                return false;

            foreach (var token in tokens)
                token.Revoked = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}