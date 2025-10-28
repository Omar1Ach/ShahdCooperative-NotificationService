using MediatR;
using ShahdCooperative.NotificationService.Application.DTOs;

namespace ShahdCooperative.NotificationService.Application.Queries.GetHealthStatus;

public class GetHealthStatusQuery : IRequest<HealthStatusDto>
{
}
