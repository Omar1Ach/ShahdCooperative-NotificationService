using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Entities;

public class InAppNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public InAppNotificationType Type { get; set; }
    public string? Category { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
}
