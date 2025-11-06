using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.Template;

public class UpdateTemplateCommandHandlerTests
{
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly UpdateTemplateCommandHandler _handler;

    public UpdateTemplateCommandHandlerTests()
    {
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _handler = new UpdateTemplateCommandHandler(_mockTemplateRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Template_And_Return_True()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Updated Template",
            Subject = "Updated Subject",
            BodyTemplate = "Updated Body",
            IsActive = false
        };

        _mockTemplateRepository.Setup(x => x.UpdateAsync(It.IsAny<NotificationTemplate>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockTemplateRepository.Verify(x => x.UpdateAsync(It.Is<NotificationTemplate>(t =>
            t.Key == command.TemplateKey &&
            t.Type == command.NotificationType &&
            t.Name == command.TemplateName &&
            t.Subject == command.Subject &&
            t.Body == command.BodyTemplate &&
            t.IsActive == command.IsActive
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_Template_To_Inactive()
    {
        var command = new UpdateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.SMS,
            TemplateName = "Test Template",
            Subject = "Test",
            BodyTemplate = "Body",
            IsActive = false
        };

        _mockTemplateRepository.Setup(x => x.UpdateAsync(It.IsAny<NotificationTemplate>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockTemplateRepository.Verify(x => x.UpdateAsync(It.Is<NotificationTemplate>(t =>
            t.IsActive == false
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new UpdateTemplateCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
