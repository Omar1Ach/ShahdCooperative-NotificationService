using MediatR;
using ShahdCooperative.NotificationService.Domain.Interfaces;

namespace ShahdCooperative.NotificationService.Application.Commands.Template;

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public DeleteTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByKeyAsync(request.TemplateKey, cancellationToken);

        if (template == null)
        {
            return false;
        }

        await _templateRepository.DeleteAsync(template.Id, cancellationToken);
        return true;
    }
}
