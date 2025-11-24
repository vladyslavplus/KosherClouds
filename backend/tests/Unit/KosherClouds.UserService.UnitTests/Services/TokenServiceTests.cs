using FluentAssertions;
using KosherClouds.UserService.Config;
using KosherClouds.UserService.Data;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services;
using KosherClouds.UserService.UnitTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KosherClouds.UserService.UnitTests.Services
{
    public class TokenServiceTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserDbContext _dbContext;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly TokenService _tokenService;
        private bool _disposed;

        public TokenServiceTests()
        {
            _userManagerMock = MockUserManagerFactory.Create();
            _dbContext = MockUserDbContextFactory.Create();

            _jwtOptions = Options.Create(new JwtSettings
            {
                Key = "ThisIsASecretKeyForTestingPurposesOnly123456789",
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });

            _tokenService = new TokenService(
                _userManagerMock.Object,
                _dbContext,
                _jwtOptions);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region GenerateTokensAsync Tests

        [Fact]
        public async Task GenerateTokensAsync_WithValidUser_ReturnsTokenResponse()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _tokenService.GenerateTokensAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateTokensAsync_AccessTokenContainsCorrectClaims()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Admin" });

            // Act
            var result = await _tokenService.GenerateTokensAsync(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public async Task GenerateTokensAsync_SavesRefreshTokenToDatabase()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _tokenService.GenerateTokensAsync(user);

            // Assert
            var savedToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);

            savedToken.Should().NotBeNull();
            savedToken!.UserId.Should().Be(user.Id);
            savedToken.Expires.Should().BeAfter(DateTime.UtcNow);
            savedToken.Revoked.Should().BeNull();
        }

        [Fact]
        public async Task GenerateTokensAsync_RevokesOldRefreshTokens()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var oldToken = UserTestData.CreateValidRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(oldToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            await _tokenService.GenerateTokensAsync(user);

            // Assert
            var revokedToken = await _dbContext.RefreshTokens.FindAsync(oldToken.Id);
            revokedToken.Should().NotBeNull();
            revokedToken!.Revoked.Should().NotBeNull();
            revokedToken.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task GenerateTokensAsync_WithMultipleOldTokens_RevokesAllOfThem()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var oldToken1 = UserTestData.CreateValidRefreshToken(user.Id);
            var oldToken2 = UserTestData.CreateValidRefreshToken(user.Id);
            var oldToken3 = UserTestData.CreateValidRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddRangeAsync(oldToken1, oldToken2, oldToken3);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            await _tokenService.GenerateTokensAsync(user);

            // Assert
            var allTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == user.Id)
                .ToListAsync();

            var revokedCount = allTokens.Count(rt => rt.Revoked != null);
            revokedCount.Should().Be(3); // All old tokens should be revoked
        }

        #endregion

        #region RefreshTokenAsync Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var refreshToken = UserTestData.CreateValidRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RefreshTokenAsync_RevokesOldToken()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var refreshToken = UserTestData.CreateValidRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            var revokedToken = await _dbContext.RefreshTokens.FindAsync(refreshToken.Id);
            revokedToken.Should().NotBeNull();
            revokedToken!.Revoked.Should().NotBeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(nonExistentUserId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _tokenService.RefreshTokenAsync(nonExistentUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithNoValidTokens_ReturnsNull()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ReturnsNull()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var expiredToken = UserTestData.CreateExpiredRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(expiredToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithRevokedToken_ReturnsNull()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var revokedToken = UserTestData.CreateRevokedRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(revokedToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithMultipleTokens_UsesLatestToken()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var oldToken = UserTestData.CreateValidRefreshToken(user.Id);
            oldToken.Created = DateTime.UtcNow.AddDays(-5);

            var newerToken = UserTestData.CreateValidRefreshToken(user.Id);
            newerToken.Created = DateTime.UtcNow.AddDays(-1);

            await _dbContext.RefreshTokens.AddRangeAsync(oldToken, newerToken);
            await _dbContext.SaveChangesAsync();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _tokenService.RefreshTokenAsync(user.Id);

            // Assert
            result.Should().NotBeNull();

            var revokedNewerToken = await _dbContext.RefreshTokens.FindAsync(newerToken.Id);
            revokedNewerToken!.Revoked.Should().NotBeNull();

            var revokedOldToken = await _dbContext.RefreshTokens.FindAsync(oldToken.Id);
            revokedOldToken!.Revoked.Should().NotBeNull();

            var allTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == user.Id)
                .ToListAsync();

            var activeTokens = allTokens.Where(rt => rt.Revoked == null).ToList();
            activeTokens.Should().HaveCount(1); // Only the newly generated token
        }

        #endregion

        #region RevokeRefreshTokenAsync Tests

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithValidTokens_RevokesAllAndReturnsTrue()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var token1 = UserTestData.CreateValidRefreshToken(user.Id);
            var token2 = UserTestData.CreateValidRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddRangeAsync(token1, token2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(user);

            // Assert
            result.Should().BeTrue();

            var revokedToken1 = await _dbContext.RefreshTokens.FindAsync(token1.Id);
            var revokedToken2 = await _dbContext.RefreshTokens.FindAsync(token2.Id);

            revokedToken1!.Revoked.Should().NotBeNull();
            revokedToken2!.Revoked.Should().NotBeNull();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithNoActiveTokens_ReturnsFalse()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(user);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithExpiredTokens_ReturnsFalse()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var expiredToken = UserTestData.CreateExpiredRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(expiredToken);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(user);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithAlreadyRevokedTokens_ReturnsFalse()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var revokedToken = UserTestData.CreateRevokedRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddAsync(revokedToken);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(user);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_DoesNotRevokeExpiredOrRevokedTokens()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var validToken = UserTestData.CreateValidRefreshToken(user.Id);
            var expiredToken = UserTestData.CreateExpiredRefreshToken(user.Id);
            var revokedToken = UserTestData.CreateRevokedRefreshToken(user.Id);

            await _dbContext.RefreshTokens.AddRangeAsync(validToken, expiredToken, revokedToken);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _tokenService.RevokeRefreshTokenAsync(user);

            // Assert
            result.Should().BeTrue();

            var validTokenRevoked = await _dbContext.RefreshTokens.FindAsync(validToken.Id);
            validTokenRevoked!.Revoked.Should().NotBeNull();

            // Expired and already revoked tokens should remain unchanged
            var expiredTokenCheck = await _dbContext.RefreshTokens.FindAsync(expiredToken.Id);
            expiredTokenCheck!.Revoked.Should().BeNull();

            var revokedTokenCheck = await _dbContext.RefreshTokens.FindAsync(revokedToken.Id);
            revokedTokenCheck!.Revoked.Should().NotBeNull();
        }

        #endregion
    }
}