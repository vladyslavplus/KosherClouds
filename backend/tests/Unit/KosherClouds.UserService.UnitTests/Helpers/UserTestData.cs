using Bogus;
using KosherClouds.Common.Seed;
using KosherClouds.UserService.DTOs.Auth;
using KosherClouds.UserService.DTOs.User;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Parameters;

namespace KosherClouds.UserService.UnitTests.Helpers
{
    public static class UserTestData
    {
        private static readonly Faker _faker = new Faker();

        public static ApplicationUser CreateValidUser()
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                EmailConfirmed = true,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                PhoneNumber = _faker.Phone.PhoneNumber(),
                PhoneNumberConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        public static List<ApplicationUser> CreateUserList(int count)
        {
            var users = new List<ApplicationUser>();
            for (int i = 0; i < count; i++)
            {
                users.Add(CreateValidUser());
            }
            return users;
        }

        public static ApplicationUser CreateAdminUser()
        {
            return new ApplicationUser
            {
                Id = SharedSeedData.AdminId,
                UserName = "admin",
                Email = SharedSeedData.AdminEmail,
                EmailConfirmed = true,
                FirstName = SharedSeedData.AdminFirstName,
                LastName = SharedSeedData.AdminLastName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ApplicationUser CreateManagerUser()
        {
            return new ApplicationUser
            {
                Id = SharedSeedData.ManagerId,
                UserName = "manager",
                Email = SharedSeedData.ManagerEmail,
                EmailConfirmed = true,
                FirstName = SharedSeedData.ManagerFirstName,
                LastName = SharedSeedData.ManagerLastName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ApplicationUser CreateRegularUser()
        {
            return new ApplicationUser
            {
                Id = SharedSeedData.UserId,
                UserName = "user",
                Email = SharedSeedData.UserEmail,
                EmailConfirmed = true,
                FirstName = SharedSeedData.UserFirstName,
                LastName = SharedSeedData.UserLastName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static RegisterRequest CreateValidRegisterRequest()
        {
            return new RegisterRequest
            {
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                Password = "Test@1234",
                PhoneNumber = $"+380{_faker.Random.Number(500000000, 999999999)}"
            };
        }

        public static LoginRequest CreateValidLoginRequest(string email = "test@example.com", string password = "Test@1234")
        {
            return new LoginRequest
            {
                Email = email,
                Password = password
            };
        }

        public static ForgotPasswordRequest CreateForgotPasswordRequest(string email)
        {
            return new ForgotPasswordRequest
            {
                Email = email
            };
        }

        public static ResetPasswordRequest CreateResetPasswordRequest(string token, string email)
        {
            return new ResetPasswordRequest
            {
                Token = token,
                Email = email,
                NewPassword = "NewPassword@123",
                ConfirmPassword = "NewPassword@123"
            };
        }

        public static UserParameters CreateUserParameters()
        {
            return new UserParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
        }

        public static List<ApplicationUser> CreateUsersWithDifferentProperties()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "john_doe",
                    Email = "john@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    EmailConfirmed = true,
                    PhoneNumber = "+1234567890",
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "jane_smith",
                    Email = "jane@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    EmailConfirmed = false,
                    PhoneNumber = "+0987654321",
                    PhoneNumberConfirmed = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "bob_johnson",
                    Email = "bob@example.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };
        }

        public static RefreshToken CreateValidRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateRandomToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = userId,
                Revoked = null
            };
        }

        public static RefreshToken CreateExpiredRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateRandomToken(),
                Expires = DateTime.UtcNow.AddDays(-1),
                Created = DateTime.UtcNow.AddDays(-8),
                UserId = userId,
                Revoked = null
            };
        }

        public static RefreshToken CreateRevokedRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateRandomToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = userId,
                Revoked = DateTime.UtcNow.AddMinutes(-5)
            };
        }

        public static UpdateUserRequest CreateValidUpdateUserRequest()
        {
            return new UpdateUserRequest
            {
                UserName = _faker.Internet.UserName(),
                Email = _faker.Internet.Email(),
                PhoneNumber = $"+380{_faker.Random.Number(500000000, 999999999)}"
            };
        }

        public static ChangePasswordRequest CreateValidChangePasswordRequest()
        {
            return new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };
        }

        private static string GenerateRandomToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}