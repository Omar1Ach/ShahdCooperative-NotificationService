namespace ShahdCooperative.NotificationService.Infrastructure.Configuration;

public class EmailSettings
{
    public string Provider { get; set; } = "SMTP";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;
    public string AwsSesAccessKey { get; set; } = string.Empty;
    public string AwsSesSecretKey { get; set; } = string.Empty;
    public string AwsSesRegion { get; set; } = "us-east-1";
}
