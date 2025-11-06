using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.UserPreference;

public class CreateUserPreferenceCommandHandler : IRequestHandler<CreateUserPreferenceCommand, Guid>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public CreateUserPreferenceCommandHandler(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
    }

    public async Task<Guid> Handle(CreateUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        var preference = new NotificationPreference
        {
            UserId = request.UserId,
            EmailEnabled = request.EmailEnabled,
            SmsEnabled = request.SmsEnabled,
            PushEnabled = request.PushEnabled,
            InAppEnabled = request.InAppEnabled
        };

        return await _preferenceRepository.CreateAsync(preference, cancellationToken);
    }
}
