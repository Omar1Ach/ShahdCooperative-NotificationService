using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public NotificationType Type { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public string? ErrorMessage { get; set; }

    // Computed property that returns the recipient (email or phone)
    public string? Recipient => RecipientEmail ?? RecipientPhone;
}
