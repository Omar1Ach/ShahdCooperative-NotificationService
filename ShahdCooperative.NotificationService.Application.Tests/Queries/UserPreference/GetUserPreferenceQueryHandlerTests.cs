using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.UserPreference;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.UserPreference;

public class GetUserPreferenceQueryHandlerTests
{
    private readonly Mock<INotificationPreferenceRepository> _mockPreferenceRepository;
    private readonly GetUserPreferenceQueryHandler _handler;

    public GetUserPreferenceQueryHandlerTests()
    {
        _mockPreferenceRepository = new Mock<INotificationPreferenceRepository>();
        _handler = new GetUserPreferenceQueryHandler(_mockPreferenceRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Preference_When_Found()
    {
        var userId = Guid.NewGuid();
        var expectedPreference = new NotificationPreference
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

        _mockPreferenceRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPreference);

        var query = new GetUserPreferenceQuery { UserId = userId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedPreference);
        _mockPreferenceRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Preference_Not_Found()
    {
        var userId = Guid.NewGuid();

        _mockPreferenceRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);

        var query = new GetUserPreferenceQuery { UserId = userId };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
        _mockPreferenceRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_PreferenceRepository_Is_Null()
    {
        var act = () => new GetUserPreferenceQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
