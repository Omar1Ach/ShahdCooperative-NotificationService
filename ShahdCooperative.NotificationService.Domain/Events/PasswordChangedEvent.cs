namespace ShahdCooperative.NotificationService.Domain.Events;

public class PasswordChangedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}
