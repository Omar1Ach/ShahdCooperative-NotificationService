using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.UserPreference;

public class GetUserPreferenceQueryHandler : IRequestHandler<GetUserPreferenceQuery, NotificationPreference?>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public GetUserPreferenceQueryHandler(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
    }

    public async Task<NotificationPreference?> Handle(GetUserPreferenceQuery request, CancellationToken cancellationToken)
    {
        return await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);
    }
}
