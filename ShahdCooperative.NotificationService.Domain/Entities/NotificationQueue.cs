using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationQueue
{
    public Guid Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public string? TemplateData { get; set; } // JSON
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public int AttemptCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
