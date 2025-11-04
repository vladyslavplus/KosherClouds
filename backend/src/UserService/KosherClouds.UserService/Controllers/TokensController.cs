using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TokensController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokensController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _tokenService.RefreshTokenAsync(userId.Value);
            if (result == null)
                return Unauthorized(new { message = "No valid refresh token found" });

            return Ok(result);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _tokenService.RevokeRefreshTokenAsync(new ApplicationUser { Id = userId.Value });

            if (!success)
                return Conflict(new { message = "No active refresh tokens to revoke" });

            return NoContent();
        }
    }
}