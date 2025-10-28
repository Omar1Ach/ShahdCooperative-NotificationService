using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Queries.NotificationHistory;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationHistoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationHistoryController> _logger;

    public NotificationHistoryController(IMediator mediator, ILogger<NotificationHistoryController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{recipient}")]
    public async Task<IActionResult> GetNotificationHistory(string recipient, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetNotificationHistoryQuery { Recipient = recipient };
            var history = await _mediator.Send(query, cancellationToken);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification history for recipient: {Recipient}", recipient);
            return StatusCode(500, "An error occurred while retrieving notification history");
        }
    }
}
