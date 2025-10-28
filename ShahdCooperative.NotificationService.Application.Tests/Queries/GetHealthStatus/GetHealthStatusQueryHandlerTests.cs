using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.GetHealthStatus;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.GetHealthStatus;

public class GetHealthStatusQueryHandlerTests
{
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly GetHealthStatusQueryHandler _handler;

    public GetHealthStatusQueryHandlerTests()
    {
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _handler = new GetHealthStatusQueryHandler(_mockQueueRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Healthy_Status_When_All_Checks_Pass()
    {
        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationQueue>());

        var result = await _handler.Handle(new GetHealthStatusQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Healthy");
        result.IsDatabaseHealthy.Should().BeTrue();
        result.IsQueueHealthy.Should().BeTrue();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task Handle_Should_Return_Unhealthy_Status_When_Database_Check_Fails()
    {
        _mockQueueRepository.Setup(x => x.GetPendingNotificationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(new GetHealthStatusQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Unhealthy");
        result.IsDatabaseHealthy.Should().BeFalse();
        result.IsQueueHealthy.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_Throw_When_QueueRepository_Is_Null()
    {
        var act = () => new GetHealthStatusQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
