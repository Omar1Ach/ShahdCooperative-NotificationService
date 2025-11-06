using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.UserPreference;

public class CreateUserPreferenceCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
}
