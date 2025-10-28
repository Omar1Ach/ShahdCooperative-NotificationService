using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Application.Queries.UserPreference;

public class GetUserPreferenceQuery : IRequest<NotificationPreference?>
{
    public Guid UserId { get; set; }
}
