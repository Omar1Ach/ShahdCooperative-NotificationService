using MediatR;

namespace ShahdCooperative.NotificationService.Application.Commands.Template;

public class DeleteTemplateCommand : IRequest<bool>
{
    public string TemplateKey { get; set; } = null!;
}
