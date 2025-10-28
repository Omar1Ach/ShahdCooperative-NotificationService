using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Queries.GetHealthStatus;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IMediator mediator, ILogger<HealthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        try
        {
            var healthStatus = await _mediator.Send(new GetHealthStatusQuery(), cancellationToken);

            if (healthStatus.Status == "Healthy")
            {
                return Ok(healthStatus);
            }
            else
            {
                return StatusCode(503, healthStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health status");
            return StatusCode(500, "An error occurred while checking health status");
        }
    }
}
