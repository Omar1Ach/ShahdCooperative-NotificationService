using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.Template;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, bool>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public UpdateTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<bool> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
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

        await _templateRepository.UpdateAsync(template, cancellationToken);
        return true;
    }
}
