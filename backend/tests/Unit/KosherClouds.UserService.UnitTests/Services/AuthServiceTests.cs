using FluentAssertions;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services;
using KosherClouds.UserService.Services.Interfaces;
using KosherClouds.UserService.UnitTests.Helpers;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace KosherClouds.UserService.UnitTests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;
        private bool _disposed;

        public AuthServiceTests()
        {
            _userManagerMock = MockUserManagerFactory.Create();
            _signInManagerMock = MockSignInManagerFactory.Create(_userManagerMock);
            _tokenServiceMock = new Mock<ITokenService>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _publishEndpointMock.Object,
                _loggerMock.Object);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cleanup if needed
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidRequest_ReturnsSuccessAndTokens()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            var mockTokens = new DTOs.Token.TokenResponse("access-token", "refresh-token");

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Setup empty Users list (no existing phone numbers)
            _userManagerMock.SetupUsers(new List<ApplicationUser>());

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.GenerateTokensAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(mockTokens);

            // Act
            var (success, error, tokens) = await _authService.RegisterAsync(registerRequest);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            tokens.Should().NotBeNull();
            tokens!.AccessToken.Should().Be("access-token");

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<Contracts.Users.UserRegisteredEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            var existingUser = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync(existingUser);

            // Act
            var (success, error, tokens) = await _authService.RegisterAsync(registerRequest);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Email already registered");
            tokens.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WhenCreateFails_ReturnsFailureWithErrors()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            var identityErrors = new[]
            {
                new IdentityError { Description = "Password too weak" },
                new IdentityError { Description = "Username already taken" }
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Setup empty Users list
            _userManagerMock.SetupUsers(new List<ApplicationUser>());

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var (success, error, tokens) = await _authService.RegisterAsync(registerRequest);

            // Assert
            success.Should().BeFalse();
            error.Should().Contain("Password too weak");
            error.Should().Contain("Username already taken");
            tokens.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithExistingPhoneNumber_ReturnsFailure()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            var existingUser = UserTestData.CreateValidUser();
            existingUser.PhoneNumber = registerRequest.PhoneNumber;

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Setup Users list with existing PhoneNumber
            _userManagerMock.SetupUsers(new List<ApplicationUser> { existingUser });

            // Act
            var (success, error, tokens) = await _authService.RegisterAsync(registerRequest);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Phone number already registered");
            tokens.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_SplitsUserNameIntoFirstAndLastName()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            registerRequest.UserName = "John Doe";
            var mockTokens = new DTOs.Token.TokenResponse("access-token", "refresh-token");

            ApplicationUser? capturedUser = null;

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock.SetupUsers(new List<ApplicationUser>());

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
                .Callback<ApplicationUser, string>((user, pass) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.GenerateTokensAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(mockTokens);

            // Act
            await _authService.RegisterAsync(registerRequest);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.FirstName.Should().Be("John");
            capturedUser.LastName.Should().Be("Doe");
            capturedUser.PhoneNumber.Should().Be(registerRequest.PhoneNumber);
            capturedUser.PhoneNumberConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterAsync_WithSingleNameUserName_SetsOnlyFirstName()
        {
            // Arrange
            var registerRequest = UserTestData.CreateValidRegisterRequest();
            registerRequest.UserName = "John";
            var mockTokens = new DTOs.Token.TokenResponse("access-token", "refresh-token");

            ApplicationUser? capturedUser = null;

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(registerRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            _userManagerMock.SetupUsers(new List<ApplicationUser>());

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
                .Callback<ApplicationUser, string>((user, pass) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.GenerateTokensAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(mockTokens);

            // Act
            await _authService.RegisterAsync(registerRequest);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.FirstName.Should().Be("John");
            capturedUser.LastName.Should().BeNull();
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessAndTokens()
        {
            // Arrange
            var loginRequest = UserTestData.CreateValidLoginRequest();
            var user = UserTestData.CreateValidUser();
            user.Email = loginRequest.Email;
            var mockTokens = new DTOs.Token.TokenResponse("access-token", "refresh-token");

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _tokenServiceMock
                .Setup(x => x.GenerateTokensAsync(user))
                .ReturnsAsync(mockTokens);

            // Act
            var (success, error, tokens) = await _authService.LoginAsync(loginRequest);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            tokens.Should().NotBeNull();
            tokens!.AccessToken.Should().Be("access-token");
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentEmail_ReturnsFailure()
        {
            // Arrange
            var loginRequest = UserTestData.CreateValidLoginRequest();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var (success, error, tokens) = await _authService.LoginAsync(loginRequest);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Invalid credentials");
            tokens.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
        {
            // Arrange
            var loginRequest = UserTestData.CreateValidLoginRequest();
            var user = UserTestData.CreateValidUser();
            user.Email = loginRequest.Email;

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginRequest.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var (success, error, tokens) = await _authService.LoginAsync(loginRequest);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Invalid credentials");
            tokens.Should().BeNull();
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_ReturnsSuccessAndPublishesEvent()
        {
            // Arrange
            var request = UserTestData.CreateForgotPasswordRequest("user@example.com");
            var user = UserTestData.CreateValidUser();
            user.Email = request.Email;

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token-123");

            // Act
            var (success, error) = await _authService.ForgotPasswordAsync(request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<Contracts.Users.PasswordResetRequestedEvent>(), default),
                Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsSuccessButDoesNotPublish()
        {
            // Arrange
            var request = UserTestData.CreateForgotPasswordRequest("nonexistent@example.com");

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var (success, error) = await _authService.ForgotPasswordAsync(request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<Contracts.Users.PasswordResetRequestedEvent>(), default),
                Times.Never);
        }

        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var encodedToken = "VGVzdFRva2Vu"; // Base64 encoded "TestToken"
            var request = UserTestData.CreateResetPasswordRequest(encodedToken, user.Email!);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (success, error) = await _authService.ResetPasswordAsync(request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public async Task ResetPasswordAsync_WithInvalidEmail_ReturnsFailure()
        {
            // Arrange
            var request = UserTestData.CreateResetPasswordRequest("token", "invalid@example.com");

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var (success, error) = await _authService.ResetPasswordAsync(request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Invalid request");
        }

        [Fact]
        public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var request = UserTestData.CreateResetPasswordRequest("invalid-token!!!", user.Email!);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act
            var (success, error) = await _authService.ResetPasswordAsync(request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Invalid token");
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenResetFails_ReturnsFailureWithErrors()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var encodedToken = "VGVzdFRva2Vu";
            var request = UserTestData.CreateResetPasswordRequest(encodedToken, user.Email!);
            var identityErrors = new[]
            {
                new IdentityError { Description = "Password is too weak" }
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var (success, error) = await _authService.ResetPasswordAsync(request);

            // Assert
            success.Should().BeFalse();
            error.Should().Contain("Password is too weak");
        }

        #endregion
    }
}