using KosherClouds.ServiceDefaults.Extensions;
using KosherClouds.UserService.DTOs.User;
using KosherClouds.UserService.Parameters;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserParameters parameters, CancellationToken cancellationToken)
        {
            var users = await _userService.GetUsersAsync(parameters, cancellationToken);
            Response.Headers["X-Pagination"] = System.Text.Json.JsonSerializer.Serialize(new
            {
                users.TotalCount,
                users.PageSize,
                users.CurrentPage,
                users.TotalPages,
                users.HasNext,
                users.HasPrevious
            });
            return Ok(users);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _userService.GetUserProfileAsync(userId.Value, cancellationToken);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("{id:guid}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserPublicInfo(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserPublicInfoAsync(id, cancellationToken);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetUserId();
            var isAdmin = User.IsAdmin();

            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            var (success, error) = await _userService.UpdateUserAsync(id, request, cancellationToken);

            if (!success)
                return BadRequest(new { message = error });

            var updatedProfile = await _userService.GetUserProfileAsync(id, cancellationToken);
            return Ok(updatedProfile);
        }

        [HttpPost("{id:guid}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetUserId();

            if (currentUserId != id)
            {
                return Forbid();
            }

            var (success, error) = await _userService.ChangePasswordAsync(id, request, cancellationToken);

            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var success = await _userService.DeleteUserAsync(id, cancellationToken);
            return success ? NoContent() : NotFound();
        }
    }
}