using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Commands.SendNotification;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var notificationId = await _mediator.Send(command, cancellationToken);
            return Ok(new { notificationId, message = "Notification queued successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, "An error occurred while sending the notification");
        }
    }
}
