using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Queries.NotificationHistory;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Queries.NotificationHistory;

public class GetNotificationHistoryQueryHandlerTests
{
    private readonly Mock<INotificationLogRepository> _mockLogRepository;
    private readonly GetNotificationHistoryQueryHandler _handler;

    public GetNotificationHistoryQueryHandlerTests()
    {
        _mockLogRepository = new Mock<INotificationLogRepository>();
        _handler = new GetNotificationHistoryQueryHandler(_mockLogRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Notification_History_For_Recipient()
    {
        var recipient = "user@example.com";
        var expectedLogs = new List<NotificationLog>
        {
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.Email,
                RecipientEmail = recipient,
                Subject = "Welcome",
                Message = "Welcome message",
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new NotificationLog
            {
                Id = Guid.NewGuid(),
                Type = NotificationType.SMS,
                RecipientPhone = recipient,
                Subject = null,
                Message = "SMS message",
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockLogRepository.Setup(x => x.GetLogsByRecipientAsync(recipient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLogs);

        var query = new GetNotificationHistoryQuery { Recipient = recipient };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedLogs);
        _mockLogRepository.Verify(x => x.GetLogsByRecipientAsync(recipient, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_History()
    {
        var recipient = "nohistory@example.com";

        _mockLogRepository.Setup(x => x.GetLogsByRecipientAsync(recipient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationLog>());

        var query = new GetNotificationHistoryQuery { Recipient = recipient };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockLogRepository.Verify(x => x.GetLogsByRecipientAsync(recipient, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_LogRepository_Is_Null()
    {
        var act = () => new GetNotificationHistoryQueryHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
