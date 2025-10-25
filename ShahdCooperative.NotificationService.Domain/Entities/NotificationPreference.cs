namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = true;
    public bool OrderUpdates { get; set; } = true;
    public bool SecurityAlerts { get; set; } = true;
    public bool NewsletterSubscription { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
