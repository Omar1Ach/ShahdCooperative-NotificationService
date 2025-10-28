using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Application.Queries.UserPreference;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserPreferencesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserPreferencesController> _logger;

    public UserPreferencesController(IMediator mediator, ILogger<UserPreferencesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserPreferences(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUserPreferenceQuery { UserId = userId };
            var preferences = await _mediator.Send(query, cancellationToken);

            if (preferences == null)
            {
                return NotFound($"Preferences for user '{userId}' not found");
            }

            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preferences for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user preferences");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserPreferences([FromBody] CreateUserPreferenceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var preferenceId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetUserPreferences), new { userId = command.UserId }, new { id = preferenceId, userId = command.UserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating preferences for user: {UserId}", command.UserId);
            return StatusCode(500, "An error occurred while creating user preferences");
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserPreferences(Guid userId, [FromBody] UpdateUserPreferenceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (userId != command.UserId)
            {
                return BadRequest("User ID in URL does not match user ID in body");
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Preferences for user '{userId}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while updating user preferences");
        }
    }
}
