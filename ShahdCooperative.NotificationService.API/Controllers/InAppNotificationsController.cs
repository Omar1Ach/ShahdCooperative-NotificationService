using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Commands.InAppNotification;
using ShahdCooperative.NotificationService.Application.Queries.InAppNotification;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InAppNotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InAppNotificationsController> _logger;

    public InAppNotificationsController(IMediator mediator, ILogger<InAppNotificationsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetUserNotificationsQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var notifications = await _mediator.Send(query, cancellationToken);

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    [HttpGet("user/{userId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUnreadCountQuery { UserId = userId };
            var count = await _mediator.Send(query, cancellationToken);

            return Ok(new { userId, unreadCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread count for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving unread count");
        }
    }

    [HttpPut("{notificationId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new MarkAsReadCommand { NotificationId = notificationId };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Notification '{notificationId}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return StatusCode(500, "An error occurred while marking notification as read");
        }
    }

    [HttpPut("user/{userId}/mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new MarkAllAsReadCommand { UserId = userId };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"User '{userId}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while marking all notifications as read");
        }
    }
}
