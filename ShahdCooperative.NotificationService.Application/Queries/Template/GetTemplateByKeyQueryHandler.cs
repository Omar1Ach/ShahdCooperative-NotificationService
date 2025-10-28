using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Queries.Template;

public class GetTemplateByKeyQueryHandler : IRequestHandler<GetTemplateByKeyQuery, NotificationTemplate?>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public GetTemplateByKeyQueryHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<NotificationTemplate?> Handle(GetTemplateByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _templateRepository.GetByKeyAsync(request.TemplateKey, cancellationToken);
    }
}
