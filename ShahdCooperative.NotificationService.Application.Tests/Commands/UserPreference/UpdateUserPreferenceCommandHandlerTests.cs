using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.UserPreference;

public class UpdateUserPreferenceCommandHandlerTests
{
    private readonly Mock<INotificationPreferenceRepository> _mockPreferenceRepository;
    private readonly UpdateUserPreferenceCommandHandler _handler;

    public UpdateUserPreferenceCommandHandlerTests()
    {
        _mockPreferenceRepository = new Mock<INotificationPreferenceRepository>();
        _handler = new UpdateUserPreferenceCommandHandler(_mockPreferenceRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Preference_And_Return_True()
    {
        var userId = Guid.NewGuid();
        var existingPreference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailNotifications = true,
            SmsNotifications = false,
            PushNotifications = true,
            InAppNotifications = true,
            MarketingEmails = true,
            OrderUpdates = true,
            SecurityAlerts = true,
            NewsletterSubscription = false
        };

        var command = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = false,
            SmsNotifications = true,
            PushNotifications = false,
            InAppNotifications = true,
            MarketingEmails = false,
            OrderUpdates = true,
            SecurityAlerts = true,
            NewsletterSubscription = true
        };

        _mockPreferenceRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPreference);

        _mockPreferenceRepository.Setup(x => x.UpdateAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockPreferenceRepository.Verify(x => x.UpdateAsync(It.Is<NotificationPreference>(p =>
            p.UserId == command.UserId &&
            p.EmailNotifications == command.EmailNotifications &&
            p.SmsNotifications == command.SmsNotifications &&
            p.PushNotifications == command.PushNotifications &&
            p.InAppNotifications == command.InAppNotifications &&
            p.MarketingEmails == command.MarketingEmails &&
            p.OrderUpdates == command.OrderUpdates &&
            p.SecurityAlerts == command.SecurityAlerts &&
            p.NewsletterSubscription == command.NewsletterSubscription
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Preference_Not_Found()
    {
        var userId = Guid.NewGuid();
        var command = new UpdateUserPreferenceCommand
        {
            UserId = userId,
            EmailNotifications = false
        };

        _mockPreferenceRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _mockPreferenceRepository.Verify(x => x.UpdateAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Constructor_Should_Throw_When_PreferenceRepository_Is_Null()
    {
        var act = () => new UpdateUserPreferenceCommandHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
