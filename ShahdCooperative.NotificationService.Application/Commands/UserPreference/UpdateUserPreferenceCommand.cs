using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.UserPreference;

public class UpdateUserPreferenceCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool InAppEnabled { get; set; }
}
