using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KosherClouds.ServiceDefaults.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(idClaim, out var id) ? id : null;
        }

        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Email);
        }

        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name)
                ?? user.Identity?.Name;
        }
    }
}
