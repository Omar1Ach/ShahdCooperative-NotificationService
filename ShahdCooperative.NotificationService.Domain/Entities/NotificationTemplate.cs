using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
