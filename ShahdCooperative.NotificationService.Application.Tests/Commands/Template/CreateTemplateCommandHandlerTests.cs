using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.Template;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.Template;

public class CreateTemplateCommandHandlerTests
{
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly CreateTemplateCommandHandler _handler;

    public CreateTemplateCommandHandlerTests()
    {
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _handler = new CreateTemplateCommandHandler(_mockTemplateRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Template_And_Return_Id()
    {
        var command = new CreateTemplateCommand
        {
            TemplateKey = "test-template",
            NotificationType = NotificationType.Email,
            TemplateName = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            IsActive = true
        };

        var expectedId = Guid.NewGuid();
        _mockTemplateRepository.Setup(x => x.CreateAsync(It.IsAny<NotificationTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockTemplateRepository.Verify(x => x.CreateAsync(It.Is<NotificationTemplate>(t =>
            t.TemplateKey == command.TemplateKey &&
            t.NotificationType == command.NotificationType &&
            t.TemplateName == command.TemplateName &&
            t.Subject == command.Subject &&
            t.BodyTemplate == command.BodyTemplate &&
            t.IsActive == command.IsActive
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_Template_Without_Subject()
    {
        var command = new CreateTemplateCommand
        {
            TemplateKey = "sms-template",
            NotificationType = NotificationType.SMS,
            TemplateName = "SMS Template",
            Subject = null,
            BodyTemplate = "SMS Body",
            IsActive = true
        };

        var expectedId = Guid.NewGuid();
        _mockTemplateRepository.Setup(x => x.CreateAsync(It.IsAny<NotificationTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockTemplateRepository.Verify(x => x.CreateAsync(It.Is<NotificationTemplate>(t =>
            t.Subject == string.Empty
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_TemplateRepository_Is_Null()
    {
        var act = () => new CreateTemplateCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
