using MediatR;
using ShahdCooperative.NotificationService.Domain.Entities;

namespace ShahdCooperative.NotificationService.Application.Queries.Template;

public class GetAllTemplatesQuery : IRequest<IEnumerable<NotificationTemplate>>
{
}
