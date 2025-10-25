using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Entities;
using ShahdCooperative.NotificationService.Domain.Enums;

namespace ShahdCooperative.NotificationService.Domain.Tests.Entities;

public class InAppNotificationTests
{
    [Fact]
    public void InAppNotification_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var notification = new InAppNotification();

        // Assert
        notification.Id.Should().Be(Guid.Empty);
        notification.UserId.Should().Be(Guid.Empty);
        notification.Title.Should().BeEmpty();
        notification.Message.Should().BeEmpty();
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void InAppNotification_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var title = "New Order";
        var message = "Your order has been placed";
        var category = "Order";
        var actionUrl = "/orders/123";

        // Act
        var notification = new InAppNotification
        {
            Id = id,
            UserId = userId,
            Title = title,
            Message = message,
            Type = InAppNotificationType.Success,
            Category = category,
            ActionUrl = actionUrl
        };

        // Assert
        notification.Id.Should().Be(id);
        notification.UserId.Should().Be(userId);
        notification.Title.Should().Be(title);
        notification.Message.Should().Be(message);
        notification.Type.Should().Be(InAppNotificationType.Success);
        notification.Category.Should().Be(category);
        notification.ActionUrl.Should().Be(actionUrl);
    }

    [Fact]
    public void InAppNotification_Should_Be_Expired_When_ExpiresAt_Is_Past()
    {
        // Arrange
        var notification = new InAppNotification
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        notification.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void InAppNotification_Should_Not_Be_Expired_When_ExpiresAt_Is_Future()
    {
        // Arrange
        var notification = new InAppNotification
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        notification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void InAppNotification_Should_Not_Be_Expired_When_ExpiresAt_Is_Null()
    {
        // Arrange
        var notification = new InAppNotification
        {
            ExpiresAt = null
        };

        // Act & Assert
        notification.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void InAppNotification_Should_Mark_As_Read()
    {
        // Arrange
        var notification = new InAppNotification
        {
            IsRead = false,
            ReadAt = null
        };

        // Act
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(InAppNotificationType.Info)]
    [InlineData(InAppNotificationType.Success)]
    [InlineData(InAppNotificationType.Warning)]
    [InlineData(InAppNotificationType.Error)]
    public void InAppNotification_Should_Support_All_Types(InAppNotificationType type)
    {
        // Arrange & Act
        var notification = new InAppNotification
        {
            Type = type
        };

        // Assert
        notification.Type.Should().Be(type);
    }
}
