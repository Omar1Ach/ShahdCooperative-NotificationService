using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using ShahdCooperative.NotificationService.Infrastructure.Services.InApp;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services.InApp;

public class InAppNotificationSenderTests
{
    private readonly Mock<ILogger<InAppNotificationSender>> _mockLogger;
    private readonly Mock<INotificationHubClient> _mockHubClient;
    private readonly Mock<IInAppNotificationRepository> _mockRepository;
    private readonly ServiceProvider _serviceProvider;

    public InAppNotificationSenderTests()
    {
        _mockLogger = new Mock<ILogger<InAppNotificationSender>>();
        _mockHubClient = new Mock<INotificationHubClient>();
        _mockRepository = new Mock<IInAppNotificationRepository>();

        var services = new ServiceCollection();
        services.AddScoped(_ => _mockRepository.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new InAppNotificationSender(null!, _serviceProvider, _mockHubClient.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_ServiceProvider_Is_Null()
    {
        var act = () => new InAppNotificationSender(_mockLogger.Object, null!, _mockHubClient.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_HubClient_Is_Null()
    {
        var act = () => new InAppNotificationSender(_mockLogger.Object, _serviceProvider, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotificationType_Should_Return_InApp()
    {
        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        sender.NotificationType.Should().Be(NotificationType.InApp);
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Empty()
    {
        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        var result = await sender.SendAsync("", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Body_Is_Empty()
    {
        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        var result = await sender.SendAsync(Guid.NewGuid().ToString(), "Subject", "");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Recipient_Is_Invalid_Guid()
    {
        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        var result = await sender.SendAsync("invalid-guid", "Subject", "Body");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_Should_Return_True_When_Notification_Sent_Successfully()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<InAppNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notificationId);

        _mockHubClient.Setup(x => x.SendNotificationToUserAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        var result = await sender.SendAsync(userId.ToString(), "Test Subject", "Test Body");

        result.Should().BeTrue();
        _mockRepository.Verify(x => x.CreateAsync(
            It.Is<InAppNotification>(n =>
                n.UserId == userId &&
                n.Title == "Test Subject" &&
                n.Message == "Test Body" &&
                n.IsRead == false),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockHubClient.Verify(x => x.SendNotificationToUserAsync(userId.ToString(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Should_Return_False_When_Repository_Throws_Exception()
    {
        var userId = Guid.NewGuid();

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<InAppNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var sender = new InAppNotificationSender(_mockLogger.Object, _serviceProvider, _mockHubClient.Object);

        var result = await sender.SendAsync(userId.ToString(), "Test Subject", "Test Body");

        result.Should().BeFalse();
    }
}
