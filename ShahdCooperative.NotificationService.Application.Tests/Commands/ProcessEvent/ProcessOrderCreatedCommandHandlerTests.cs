using FluentAssertions;
using Moq;
using ShahdCooperative.NotificationService.Application.Commands.ProcessEvent;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;
using ShahdCooperative.NotificationService.Domain.Events;
using ShahdCooperative.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShahdCooperative.NotificationService.Application.Tests.Commands.ProcessEvent;

public class ProcessOrderCreatedCommandHandlerTests
{
    private readonly Mock<INotificationQueueRepository> _mockQueueRepository;
    private readonly ProcessOrderCreatedCommandHandler _handler;

    public ProcessOrderCreatedCommandHandlerTests()
    {
        _mockQueueRepository = new Mock<INotificationQueueRepository>();
        _handler = new ProcessOrderCreatedCommandHandler(_mockQueueRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Enqueue_Order_Confirmation_Email()
    {
        var orderId = Guid.NewGuid();
        var command = new ProcessOrderCreatedCommand
        {
            Event = new OrderCreatedEvent
            {
                OrderId = orderId,
                UserId = Guid.NewGuid(),
                Email = "customer@example.com",
                TotalAmount = 299.99m,
                ItemCount = 3,
                OrderDate = DateTime.UtcNow
            }
        };

        NotificationQueue? capturedNotification = null;
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationQueue, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _mockQueueRepository.Verify(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedNotification.Should().NotBeNull();
        capturedNotification!.NotificationType.Should().Be(NotificationType.Email);
        capturedNotification.Recipient.Should().Be("customer@example.com");
        capturedNotification.Subject.Should().Contain("Order Confirmation");
        capturedNotification.TemplateKey.Should().Be("order-created");
        capturedNotification.Priority.Should().Be(NotificationPriority.High);
    }

    [Fact]
    public async Task Handle_Should_Include_Order_Details_In_TemplateData()
    {
        var orderId = Guid.NewGuid();
        var command = new ProcessOrderCreatedCommand
        {
            Event = new OrderCreatedEvent
            {
                OrderId = orderId,
                UserId = Guid.NewGuid(),
                Email = "customer@example.com",
                TotalAmount = 299.99m,
                ItemCount = 3,
                OrderDate = DateTime.UtcNow
            }
        };

        NotificationQueue? capturedNotification = null;
        _mockQueueRepository.Setup(x => x.EnqueueAsync(It.IsAny<NotificationQueue>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationQueue, CancellationToken>((notification, _) => capturedNotification = notification)
            .ReturnsAsync(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        capturedNotification!.TemplateData.Should().Contain(orderId.ToString());
        capturedNotification.TemplateData.Should().Contain("customer@example.com");
    }
}
