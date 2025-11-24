using KosherClouds.UserService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KosherClouds.UserService.UnitTests.Helpers
{
    public static class MockUserManagerFactory
    {
        public static Mock<UserManager<ApplicationUser>> Create()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var hasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
            var validatorsMock = new List<IUserValidator<ApplicationUser>>();
            var passwordValidatorsMock = new List<IPasswordValidator<ApplicationUser>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var errorsMock = new Mock<IdentityErrorDescriber>();
            var servicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                optionsMock.Object,
                hasherMock.Object,
                validatorsMock,
                passwordValidatorsMock,
                lookupNormalizerMock.Object,
                errorsMock.Object,
                servicesMock.Object,
                loggerMock.Object);

            return userManager;
        }
    }
}
