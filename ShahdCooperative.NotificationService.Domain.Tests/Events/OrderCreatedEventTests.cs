using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Events;
using Xunit;

namespace ShahdCooperative.NotificationService.Domain.Tests.Events;

public class OrderCreatedEventTests
{
    [Fact]
    public void OrderCreatedEvent_Should_Initialize_With_Default_Values()
    {
        var ev = new OrderCreatedEvent();

        ev.OrderId.Should().Be(Guid.Empty);
        ev.UserId.Should().Be(Guid.Empty);
        ev.Email.Should().BeEmpty();
        ev.TotalAmount.Should().Be(0);
        ev.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        ev.ItemCount.Should().Be(0);
    }

    [Fact]
    public void OrderCreatedEvent_Should_Allow_Property_Setting()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow.AddDays(-1);

        var ev = new OrderCreatedEvent
        {
            OrderId = orderId,
            UserId = userId,
            Email = "customer@example.com",
            TotalAmount = 199.99m,
            OrderDate = orderDate,
            ItemCount = 5
        };

        ev.OrderId.Should().Be(orderId);
        ev.UserId.Should().Be(userId);
        ev.Email.Should().Be("customer@example.com");
        ev.TotalAmount.Should().Be(199.99m);
        ev.OrderDate.Should().Be(orderDate);
        ev.ItemCount.Should().Be(5);
    }
}
