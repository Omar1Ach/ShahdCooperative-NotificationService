using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.API.Controllers;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Application.Queries.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using Xunit;

namespace ShahdCooperative.NotificationService.API.Tests.Controllers;

public class TemplatesControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<TemplatesController>> _mockLogger;
    private readonly TemplatesController _controller;

    public TemplatesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<TemplatesController>>();
        _controller = new TemplatesController(_mockMediator.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Mediator_Is_Null()
    {
        var act = () => new TemplatesController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new TemplatesController(_mockMediator.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllTemplates_Should_Return_Ok_With_Templates()
    {
        var templates = new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Key = "template-1",
                Type = NotificationType.Email,
                Name = "Template 1",
                Body = "Body 1"
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetAllTemplatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await _controller.GetAllTemplates(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(templates);
    }

    [Fact]
    public async Task GetAllTemplates_Should_Return_500_On_Exception()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<GetAllTemplatesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetAllTemplates(CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetTemplateByKey_Should_Return_Ok_When_Template_Found()
    {
        var template = new NotificationTemplate
        {
            Key = "test-template",
            Type = NotificationType.Email,
            Name = "Test Template",
            Body = "Test Body"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetTemplateByKeyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var result = await _controller.GetTemplateByKey("test-template", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(template);
    }

    [Fact]
    public async Task GetTemplateByKey_Should_Return_NotFound_When_Template_Not_Found()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTemplateByKeyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var result = await _controller.GetTemplateByKey("non-existent", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetTemplateByKey_Should_Return_500_On_Exception()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTemplateByKeyQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetTemplateByKey("test", CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateTemplate_Should_Return_CreatedAtAction()
    {
        var command = new CreateTemplateCommand
        {
            TemplateKey = "new-template",
            NotificationType = NotificationType.Email,
            TemplateName = "New Template",
            BodyTemplate = "New Body"
        };

        var templateId = Guid.NewGuid();
        _mockMediator.Setup(x => x.Send(It.IsAny<CreateTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(templateId);

        var result = await _controller.CreateTemplate(command, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(TemplatesController.GetTemplateByKey));
    }

    [Fact]
    public async Task CreateTemplate_Should_Return_500_On_Exception()
    {
        var command = new CreateTemplateCommand
        {
            TemplateKey = "new-template",
            NotificationType = NotificationType.Email,
            TemplateName = "New Template",
            BodyTemplate = "New Body"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.CreateTemplate(command, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateTemplate_Should_Return_NoContent_When_Successful()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Updated Template",
            BodyTemplate = "Updated Body"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.UpdateTemplate("test-template", command, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateTemplate_Should_Return_BadRequest_When_Keys_Dont_Match()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "template-1",
            NotificationType = NotificationType.Email,
            TemplateName = "Updated Template",
            BodyTemplate = "Updated Body"
        };

        var result = await _controller.UpdateTemplate("template-2", command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateTemplate_Should_Return_NotFound_When_Template_Not_Found()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Updated Template",
            BodyTemplate = "Updated Body"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.UpdateTemplate("test-template", command, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateTemplate_Should_Return_500_On_Exception()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Updated Template",
            BodyTemplate = "Updated Body"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateTemplate("test-template", command, CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteTemplate_Should_Return_NoContent_When_Successful()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteTemplate("test-template", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTemplate_Should_Return_NotFound_When_Template_Not_Found()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.DeleteTemplate("non-existent", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTemplate_Should_Return_500_On_Exception()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.DeleteTemplate("test-template", CancellationToken.None);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
