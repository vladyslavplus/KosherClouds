namespace KosherClouds.UserService.DTOs.User
{
    public class UserPublicDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string DisplayName => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName} {LastName}"
            : UserName ?? "Unknown User";
    }
}
