using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Application.Queries.NotificationHistory;

public class GetNotificationHistoryQuery : IRequest<IEnumerable<NotificationLog>>
{
    public string Recipient { get; set; } = null!;
}
