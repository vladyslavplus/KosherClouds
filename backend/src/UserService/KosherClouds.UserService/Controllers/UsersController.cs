using KosherClouds.UserService.Parameters;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KosherClouds.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
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

        [HttpGet("{id:guid}")]
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

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var success = await _userService.DeleteUserAsync(id, cancellationToken);
            return success ? NoContent() : NotFound();
        }
    }
}