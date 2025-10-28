using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.Template;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public CreateTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new NotificationTemplate
        {
            TemplateKey = request.TemplateKey,
            NotificationType = request.NotificationType,
            TemplateName = request.TemplateName,
            Subject = request.Subject ?? string.Empty,
            BodyTemplate = request.BodyTemplate,
            IsActive = request.IsActive
        };

        return await _templateRepository.CreateAsync(template, cancellationToken);
    }
}
