using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.UserPreference;

public class UpdateUserPreferenceCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool InAppNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public bool OrderUpdates { get; set; }
    public bool SecurityAlerts { get; set; }
    public bool NewsletterSubscription { get; set; }
}
