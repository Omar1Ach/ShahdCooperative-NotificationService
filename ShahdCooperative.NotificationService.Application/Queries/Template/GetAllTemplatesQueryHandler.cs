using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.Template;

public class GetAllTemplatesQueryHandler : IRequestHandler<GetAllTemplatesQuery, IEnumerable<NotificationTemplate>>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public GetAllTemplatesQueryHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<IEnumerable<NotificationTemplate>> Handle(GetAllTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _templateRepository.GetAllAsync(cancellationToken);
    }
}
