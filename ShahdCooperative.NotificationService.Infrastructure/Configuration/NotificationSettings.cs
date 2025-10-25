namespace ShahdCooperative.NotificationService.Infrastructure.Configuration;

public class NotificationSettings
{
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMinutes { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int ProcessingIntervalSeconds { get; set; } = 30;
}
