namespace KosherClouds.Contracts.Users
{
    public class PasswordResetRequestedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
