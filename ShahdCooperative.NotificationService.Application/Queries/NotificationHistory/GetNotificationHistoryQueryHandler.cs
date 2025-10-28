using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.NotificationHistory;

public class GetNotificationHistoryQueryHandler : IRequestHandler<GetNotificationHistoryQuery, IEnumerable<NotificationLog>>
{
    private readonly INotificationLogRepository _logRepository;

    public GetNotificationHistoryQueryHandler(INotificationLogRepository logRepository)
    {
        _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
    }

    public async Task<IEnumerable<NotificationLog>> Handle(GetNotificationHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _logRepository.GetLogsByRecipientAsync(request.Recipient, cancellationToken);
    }
}
