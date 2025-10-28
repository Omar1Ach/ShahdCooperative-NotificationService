using MediatR;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Application.Commands.Template;

public class UpdateTemplateCommand : IRequest<bool>
{
    public string TemplateKey { get; set; } = null!;
    public NotificationType NotificationType { get; set; }
    public string TemplateName { get; set; } = null!;
    public string? Subject { get; set; }
    public string BodyTemplate { get; set; } = null!;
    public bool IsActive { get; set; }
}
