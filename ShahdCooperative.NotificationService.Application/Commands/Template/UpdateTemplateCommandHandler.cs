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
        // First, get the existing template by key to get its Id
        var existingTemplate = await _templateRepository.GetByKeyAsync(request.TemplateKey, cancellationToken);

        if (existingTemplate == null)
        {
            return false;
        }

        // Update the template properties
        existingTemplate.Key = request.TemplateKey;
        existingTemplate.Type = request.NotificationType;
        existingTemplate.Name = request.TemplateName;
        existingTemplate.Subject = request.Subject;
        existingTemplate.Body = request.BodyTemplate;
        existingTemplate.IsActive = request.IsActive;

        await _templateRepository.UpdateAsync(existingTemplate, cancellationToken);
        return true;
    }
}
