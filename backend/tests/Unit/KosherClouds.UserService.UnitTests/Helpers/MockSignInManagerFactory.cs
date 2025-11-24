using KosherClouds.UserService.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KosherClouds.UserService.UnitTests.Helpers
{
    public static class MockSignInManagerFactory
    {
        public static Mock<SignInManager<ApplicationUser>> Create(Mock<UserManager<ApplicationUser>> userManager)
        {
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var loggerMock = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<ApplicationUser>>();

            var signInManager = new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsMock.Object,
                loggerMock.Object,
                schemesMock.Object,
                confirmationMock.Object);

            return signInManager;
        }
    }
}
