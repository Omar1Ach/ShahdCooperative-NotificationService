using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Application.Queries.Template;

namespace ShahdCooperative.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(IMediator mediator, ILogger<TemplatesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTemplates(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAllTemplatesQuery();
            var templates = await _mediator.Send(query, cancellationToken);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    [HttpGet("{templateKey}")]
    public async Task<IActionResult> GetTemplateByKey(string templateKey, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetTemplateByKeyQuery { TemplateKey = templateKey };
            var template = await _mediator.Send(query, cancellationToken);

            if (template == null)
            {
                return NotFound($"Template with key '{templateKey}' not found");
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template with key: {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while retrieving the template");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var templateId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTemplateByKey), new { templateKey = command.TemplateKey }, new { id = templateId, templateKey = command.TemplateKey });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, "An error occurred while creating the template");
        }
    }

    [HttpPut("{templateKey}")]
    public async Task<IActionResult> UpdateTemplate(string templateKey, [FromBody] UpdateTemplateCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (templateKey != command.TemplateKey)
            {
                return BadRequest("Template key in URL does not match template key in body");
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Template with key '{templateKey}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template with key: {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while updating the template");
        }
    }

    [HttpDelete("{templateKey}")]
    public async Task<IActionResult> DeleteTemplate(string templateKey, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteTemplateCommand { TemplateKey = templateKey };
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Template with key '{templateKey}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template with key: {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while deleting the template");
        }
    }
}
