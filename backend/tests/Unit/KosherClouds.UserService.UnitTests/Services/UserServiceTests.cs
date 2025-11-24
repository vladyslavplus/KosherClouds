using FluentAssertions;
using KosherClouds.ServiceDefaults.Helpers;
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