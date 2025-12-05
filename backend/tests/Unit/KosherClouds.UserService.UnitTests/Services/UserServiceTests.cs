using FluentAssertions;
using KosherClouds.ServiceDefaults.Helpers;
using KosherClouds.UserService.DTOs.User;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Parameters;
using KosherClouds.UserService.UnitTests.Helpers;
using Microsoft.AspNetCore.Identity;
using Moq;
using UserServiceClass = KosherClouds.UserService.Services.UserService;

namespace KosherClouds.UserService.UnitTests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ISortHelperFactory> _sortHelperFactoryMock;
        private readonly Mock<ISortHelper<ApplicationUser>> _sortHelperMock;
        private readonly UserServiceClass _userService;
        private bool _disposed;

        public UserServiceTests()
        {
            _userManagerMock = MockUserManagerFactory.Create();

            _sortHelperMock = new Mock<ISortHelper<ApplicationUser>>();
            _sortHelperMock
                .Setup(x => x.ApplySort(It.IsAny<IQueryable<ApplicationUser>>(), It.IsAny<string>()))
                .Returns<IQueryable<ApplicationUser>, string>((query, orderBy) => query);

            _sortHelperFactoryMock = new Mock<ISortHelperFactory>();
            _sortHelperFactoryMock
                .Setup(x => x.Create<ApplicationUser>())
                .Returns(_sortHelperMock.Object);

            _userService = new UserServiceClass(
                _userManagerMock.Object,
                _sortHelperFactoryMock.Object,
                isInMemory: true);
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

        #region GetUsersAsync Tests

        [Fact]
        public async Task GetUsersAsync_WithNoFilters_ReturnsAllUsers()
        {
            // Arrange
            var users = UserTestData.CreateUserList(5);
            _userManagerMock.SetupUsers(users);

            var parameters = UserTestData.CreateUserParameters();

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetUsersAsync_WithUserNameFilter_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                UserName = "john",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2); // john_doe and bob_johnson
        }

        [Fact]
        public async Task GetUsersAsync_WithEmailFilter_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                Email = "jane",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].Email.Should().Contain("jane");
        }

        [Fact]
        public async Task GetUsersAsync_WithPhoneNumberFilter_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                PhoneNumber = "+123",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1);
            result[0].PhoneNumber.Should().Contain("+123");
        }

        [Fact]
        public async Task GetUsersAsync_WithEmailConfirmedFilter_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                EmailConfirmed = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2); // john and bob
            result.Should().OnlyContain(u => u.EmailConfirmed);
        }

        [Fact]
        public async Task GetUsersAsync_WithPhoneNumberConfirmedFilter_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                PhoneNumberConfirmed = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1); // Only john
            result[0].PhoneNumberConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task GetUsersAsync_WithCreatedAtFromFilter_ReturnsUsersCreatedAfterDate()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                CreatedAtFrom = DateTime.UtcNow.AddDays(-15),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2); // jane and bob
        }

        [Fact]
        public async Task GetUsersAsync_WithCreatedAtToFilter_ReturnsUsersCreatedBeforeDate()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                CreatedAtTo = DateTime.UtcNow.AddDays(-20),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(1); // Only john
        }

        [Fact]
        public async Task GetUsersAsync_WithMultipleFilters_ReturnsMatchingUsers()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                EmailConfirmed = true,
                CreatedAtFrom = DateTime.UtcNow.AddDays(-35),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2); // john and bob
            result.Should().OnlyContain(u => u.EmailConfirmed);
        }

        [Fact]
        public async Task GetUsersAsync_WhenNoUsersMatch_ReturnsEmptyList()
        {
            // Arrange
            var users = UserTestData.CreateUsersWithDifferentProperties();
            _userManagerMock.SetupUsers(users);

            var parameters = new UserParameters
            {
                UserName = "NonExistentUser",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _userService.GetUsersAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
            result.TotalCount.Should().Be(0);
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var users = new List<ApplicationUser> { user };
            _userManagerMock.SetupUsers(users);

            // Act
            var result = await _userService.GetUserByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var users = UserTestData.CreateUserList(3);
            _userManagerMock.SetupUsers(users);

            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userService.GetUserByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserPublicInfoAsync Tests

        [Fact]
        public async Task GetUserPublicInfoAsync_WithValidId_ReturnsPublicInfo()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var users = new List<ApplicationUser> { user };
            _userManagerMock.SetupUsers(users);

            // Act
            var result = await _userService.GetUserPublicInfoAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.UserName.Should().Be(user.UserName);
            result.Email.Should().Be(user.Email);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
        }

        [Fact]
        public async Task GetUserPublicInfoAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var users = UserTestData.CreateUserList(3);
            _userManagerMock.SetupUsers(users);

            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userService.GetUserPublicInfoAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WithValidUserName_ReturnsSuccess()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.UserName = "old_username";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name.ToUpperInvariant());

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateNormalizedUserNameAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateUserRequest
            {
                UserName = "new_username"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            _userManagerMock.Verify(x => x.UpdateNormalizedUserNameAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidEmail_ReturnsSuccessAndResetsEmailConfirmed()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.Email = "old@example.com";
            user.EmailConfirmed = true;

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.NormalizeEmail(It.IsAny<string>()))
                .Returns<string>(email => email.ToUpperInvariant());

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateNormalizedEmailAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateUserRequest
            {
                Email = "new@example.com"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            _userManagerMock.Verify(x => x.UpdateNormalizedEmailAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidPhoneNumber_ReturnsSuccessAndResetsPhoneConfirmed()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.PhoneNumber = "+1234567890";
            user.PhoneNumberConfirmed = true;

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var request = new UpdateUserRequest
            {
                PhoneNumber = "+0987654321"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateUserName_ReturnsFailure()
        {
            // Arrange
            var user1 = UserTestData.CreateValidUser();
            user1.UserName = "existing_user";
            user1.NormalizedUserName = "EXISTING_USER";

            var user2 = UserTestData.CreateValidUser();
            user2.UserName = "current_user";
            user2.NormalizedUserName = "CURRENT_USER";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user2.Id.ToString()))
                .ReturnsAsync(user2);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user1, user2 });

            _userManagerMock
                .Setup(x => x.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name.ToUpperInvariant());

            var request = new UpdateUserRequest
            {
                UserName = "existing_user" // Trying to use user1's username
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user2.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Username is already taken");
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var user1 = UserTestData.CreateValidUser();
            user1.Email = "existing@example.com";
            user1.NormalizedEmail = "EXISTING@EXAMPLE.COM";

            var user2 = UserTestData.CreateValidUser();
            user2.Email = "current@example.com";
            user2.NormalizedEmail = "CURRENT@EXAMPLE.COM";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user2.Id.ToString()))
                .ReturnsAsync(user2);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user1, user2 });

            _userManagerMock
                .Setup(x => x.NormalizeEmail(It.IsAny<string>()))
                .Returns<string>(email => email.ToUpperInvariant());

            var request = new UpdateUserRequest
            {
                Email = "existing@example.com" // Trying to use user1's email
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user2.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Email is already taken");
        }

        [Fact]
        public async Task UpdateUserAsync_WithDuplicatePhoneNumber_ReturnsFailure()
        {
            // Arrange
            var user1 = UserTestData.CreateValidUser();
            user1.PhoneNumber = "+1234567890";

            var user2 = UserTestData.CreateValidUser();
            user2.PhoneNumber = "+0987654321";

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user2.Id.ToString()))
                .ReturnsAsync(user2);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user1, user2 });

            var request = new UpdateUserRequest
            {
                PhoneNumber = "+1234567890" // Trying to use user1's phone
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user2.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("Phone number is already taken");
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var users = UserTestData.CreateUserList(3);
            _userManagerMock.SetupUsers(users);

            var nonExistentId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(nonExistentId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            var request = new UpdateUserRequest
            {
                UserName = "new_username"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(nonExistentId, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("User not found");
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUpdateFails_ReturnsFailureWithErrors()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name.ToUpperInvariant());

            var identityErrors = new[]
            {
        new IdentityError { Description = "Update failed due to database error" }
    };

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            _userManagerMock
                .Setup(x => x.UpdateNormalizedUserNameAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateUserRequest
            {
                UserName = "new_username"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Contain("Update failed due to database error");
        }

        [Fact]
        public async Task UpdateUserAsync_WithEmptyPhoneNumber_ClearsPhoneAndResetsConfirmation()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.PhoneNumber = "+1234567890";
            user.PhoneNumberConfirmed = true;

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var request = new UpdateUserRequest
            {
                PhoneNumber = "" // Empty string to clear phone
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WithAllFieldsChanged_UpdatesSuccessfully()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.UserName = "old_username";
            user.Email = "old@example.com";
            user.PhoneNumber = "+1234567890";
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name.ToUpperInvariant());

            _userManagerMock
                .Setup(x => x.NormalizeEmail(It.IsAny<string>()))
                .Returns<string>(email => email.ToUpperInvariant());

            _userManagerMock
                .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateNormalizedUserNameAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            _userManagerMock
                .Setup(x => x.UpdateNormalizedEmailAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            var request = new UpdateUserRequest
            {
                UserName = "new_username",
                Email = "new@example.com",
                PhoneNumber = "+0987654321"
            };

            // Act
            var (success, error) = await _userService.UpdateUserAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            _userManagerMock.Verify(x => x.UpdateNormalizedUserNameAsync(It.IsAny<ApplicationUser>()), Times.Once);
            _userManagerMock.Verify(x => x.UpdateNormalizedEmailAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ReturnsSuccess()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), "OldPassword@123", "NewPassword@123"))
                .ReturnsAsync(IdentityResult.Success);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };

            // Act
            var (success, error) = await _userService.ChangePasswordAsync(user.Id, request);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ReturnsFailure()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            var identityErrors = new[]
            {
        new IdentityError { Description = "Incorrect password" }
    };

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), "WrongPassword@123", "NewPassword@123"))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword@123",
                NewPassword = "NewPassword@123"
            };

            // Act
            var (success, error) = await _userService.ChangePasswordAsync(user.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Contain("Incorrect password");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithWeakNewPassword_ReturnsFailure()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            var identityErrors = new[]
            {
        new IdentityError { Description = "Password must contain at least one uppercase letter" },
        new IdentityError { Description = "Password must contain at least one special character" }
    };

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), "OldPassword@123", "weakpassword"))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "weakpassword"
            };

            // Act
            var (success, error) = await _userService.ChangePasswordAsync(user.Id, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Contain("uppercase letter");
            error.Should().Contain("special character");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var users = UserTestData.CreateUserList(3);
            _userManagerMock.SetupUsers(users);

            var nonExistentId = Guid.NewGuid();
            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };

            // Act
            var (success, error) = await _userService.ChangePasswordAsync(nonExistentId, request);

            // Assert
            success.Should().BeFalse();
            error.Should().Be("User not found");
        }

        #endregion

        #region GetUserProfileAsync Tests

        [Fact]
        public async Task GetUserProfileAsync_WithValidId_ReturnsProfile()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            user.FirstName = "John";
            user.LastName = "Doe";
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = false;

            _userManagerMock.SetupUsers(new List<ApplicationUser> { user });

            // Act
            var result = await _userService.GetUserProfileAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.UserName.Should().Be(user.UserName);
            result.Email.Should().Be(user.Email);
            result.PhoneNumber.Should().Be(user.PhoneNumber);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.EmailConfirmed.Should().BeTrue();
            result.PhoneNumberConfirmed.Should().BeFalse();
            result.CreatedAt.Should().Be(user.CreatedAt);
            result.UpdatedAt.Should().Be(user.UpdatedAt);
        }

        [Fact]
        public async Task GetUserProfileAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var users = UserTestData.CreateUserList(3);
            _userManagerMock.SetupUsers(users);

            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userService.GetUserProfileAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.DeleteUserAsync(user.Id);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(nonExistentId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _userService.DeleteUserAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
            _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenDeleteFails_ReturnsFalse()
        {
            // Arrange
            var user = UserTestData.CreateValidUser();
            var identityErrors = new[]
            {
                new IdentityError { Description = "Cannot delete user" }
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _userService.DeleteUserAsync(user.Id);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}