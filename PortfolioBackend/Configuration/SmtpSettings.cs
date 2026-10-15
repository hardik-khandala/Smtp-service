namespace PortfolioBackend.Configuration
{
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";

        public string Host { get; init; } = "smtp.zoho.in";
        public int Port { get; init; } = 587;
        public string SenderName { get; init; } = "Portfolio Contact Form";
        public string SenderEmail { get; init; } = string.Empty;
        public string RecipientEmail { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
