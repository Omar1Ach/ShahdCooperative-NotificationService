using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Application.Queries.Template;

public class GetTemplateByKeyQuery : IRequest<NotificationTemplate?>
{
    public string TemplateKey { get; set; } = null!;
}
