namespace KosherClouds.NotificationService.Configuration
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseSsl { get; set; } = true;
    }
}
