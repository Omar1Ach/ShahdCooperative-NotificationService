using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
