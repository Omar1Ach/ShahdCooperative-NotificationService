using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.UserPreference;

public class UpdateUserPreferenceCommandHandler : IRequestHandler<UpdateUserPreferenceCommand, bool>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public UpdateUserPreferenceCommandHandler(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
    }

    public async Task<bool> Handle(UpdateUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        var existingPreference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existingPreference == null)
        {
            return false;
        }

        existingPreference.EmailNotifications = request.EmailNotifications;
        existingPreference.SmsNotifications = request.SmsNotifications;
        existingPreference.PushNotifications = request.PushNotifications;
        existingPreference.InAppNotifications = request.InAppNotifications;
        existingPreference.MarketingEmails = request.MarketingEmails;
        existingPreference.OrderUpdates = request.OrderUpdates;
        existingPreference.SecurityAlerts = request.SecurityAlerts;
        existingPreference.NewsletterSubscription = request.NewsletterSubscription;
        existingPreference.UpdatedAt = DateTime.UtcNow;

        await _preferenceRepository.UpdateAsync(existingPreference, cancellationToken);
        return true;
    }
}
