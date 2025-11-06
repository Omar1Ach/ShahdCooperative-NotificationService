using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.UserPreference;

public class CreateUserPreferenceCommandHandlerTests
{
    private readonly Mock<INotificationPreferenceRepository> _mockPreferenceRepository;
    private readonly CreateUserPreferenceCommandHandler _handler;

    public CreateUserPreferenceCommandHandlerTests()
    {
        _mockPreferenceRepository = new Mock<INotificationPreferenceRepository>();
        _handler = new CreateUserPreferenceCommandHandler(_mockPreferenceRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Preference_And_Return_Id()
    {
        var command = new CreateUserPreferenceCommand
        {
            UserId = Guid.NewGuid(),
            EmailEnabled = true,
            SmsEnabled = false,
            PushEnabled = true,
            InAppEnabled = true
        };

        var expectedId = Guid.NewGuid();
        _mockPreferenceRepository.Setup(x => x.CreateAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockPreferenceRepository.Verify(x => x.CreateAsync(It.Is<NotificationPreference>(p =>
            p.UserId == command.UserId &&
            p.EmailEnabled == command.EmailEnabled &&
            p.SmsEnabled == command.SmsEnabled &&
            p.PushEnabled == command.PushEnabled &&
            p.InAppEnabled == command.InAppEnabled
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_Preference_With_Default_Values()
    {
        var command = new CreateUserPreferenceCommand
        {
            UserId = Guid.NewGuid()
        };

        var expectedId = Guid.NewGuid();
        _mockPreferenceRepository.Setup(x => x.CreateAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);
        _mockPreferenceRepository.Verify(x => x.CreateAsync(It.Is<NotificationPreference>(p =>
            p.EmailEnabled == true &&
            p.SmsEnabled == false &&
            p.PushEnabled == true &&
            p.InAppEnabled == true
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_PreferenceRepository_Is_Null()
    {
        var act = () => new CreateUserPreferenceCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
