using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.Template;

public class DeleteTemplateCommandHandlerTests
{
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly DeleteTemplateCommandHandler _handler;

    public DeleteTemplateCommandHandlerTests()
    {
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _handler = new DeleteTemplateCommandHandler(_mockTemplateRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Template_And_Return_True_When_Template_Found()
    {
        var command = new DeleteTemplateCommand
        {
            TemplateKey = "test-template"
        };

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = command.TemplateKey,
            NotificationType = NotificationType.Email,
            TemplateName = "Test",
            BodyTemplate = "Body"
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(command.TemplateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockTemplateRepository.Setup(x => x.DeleteAsync(template.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockTemplateRepository.Verify(x => x.GetByKeyAsync(command.TemplateKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockTemplateRepository.Verify(x => x.DeleteAsync(template.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Template_Not_Found()
    {
        var command = new DeleteTemplateCommand
        {
            TemplateKey = "non-existent"
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(command.TemplateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _mockTemplateRepository.Verify(x => x.GetByKeyAsync(command.TemplateKey, It.IsAny<CancellationToken>()), Times.Once);
        _mockTemplateRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new DeleteTemplateCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
