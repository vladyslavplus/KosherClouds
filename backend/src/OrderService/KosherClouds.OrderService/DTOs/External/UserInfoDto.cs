namespace KosherClouds.OrderService.DTOs.External
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string DisplayName => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName} {LastName}"
            : UserName ?? "Unknown User";
    }
}