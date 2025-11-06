namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
