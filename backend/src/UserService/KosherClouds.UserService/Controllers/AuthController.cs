using KosherClouds.UserService.DTOs.Auth;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, error, tokens) = await _authService.RegisterAsync(request);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(tokens);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, error, tokens) = await _authService.LoginAsync(request);
            if (!success)
                return Unauthorized(new { message = error });

            return Ok(tokens);
        }
    }
}
