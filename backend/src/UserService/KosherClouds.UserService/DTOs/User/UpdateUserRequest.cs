using System.ComponentModel.DataAnnotations;

namespace KosherClouds.UserService.DTOs.User
{
    public class UpdateUserRequest
    {
        public string? UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
