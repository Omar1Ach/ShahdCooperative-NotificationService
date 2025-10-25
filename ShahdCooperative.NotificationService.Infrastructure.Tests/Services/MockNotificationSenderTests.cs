using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Infrastructure.Services;
using Xunit;

namespace ShahdCooperative.NotificationService.Infrastructure.Tests.Services;

public class MockNotificationSenderTests
{
    private readonly Mock<ILogger<MockNotificationSender>> _mockLogger;

    public MockNotificationSenderTests()
    {
        _mockLogger = new Mock<ILogger<MockNotificationSender>>();
    }

    [Fact]
    public void Constructor_Should_Set_NotificationType()
    {
        var sender = new MockNotificationSender(_mockLogger.Object, NotificationType.Email);

        sender.NotificationType.Should().Be(NotificationType.Email);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Logger_Is_Null()
    {
        var act = () => new MockNotificationSender(null!, NotificationType.Email);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(NotificationType.Email)]
    [InlineData(NotificationType.SMS)]
    [InlineData(NotificationType.Push)]
    [InlineData(NotificationType.InApp)]
    public async Task SendAsync_Should_Return_True_For_All_Types(NotificationType notificationType)
    {
        var sender = new MockNotificationSender(_mockLogger.Object, notificationType);

        var result = await sender.SendAsync("test@example.com", "Subject", "Body");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_Should_Log_Information()
    {
        var sender = new MockNotificationSender(_mockLogger.Object, NotificationType.Email);

        await sender.SendAsync("test@example.com", "Test Subject", "Test Body");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MOCK")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
