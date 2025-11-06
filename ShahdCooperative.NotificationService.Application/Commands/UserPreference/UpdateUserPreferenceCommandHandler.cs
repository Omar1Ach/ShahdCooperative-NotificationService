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

        existingPreference.EmailEnabled = request.EmailEnabled;
        existingPreference.SmsEnabled = request.SmsEnabled;
        existingPreference.PushEnabled = request.PushEnabled;
        existingPreference.InAppEnabled = request.InAppEnabled;
        existingPreference.UpdatedAt = DateTime.UtcNow;

        await _preferenceRepository.UpdateAsync(existingPreference, cancellationToken);
        return true;
    }
}
