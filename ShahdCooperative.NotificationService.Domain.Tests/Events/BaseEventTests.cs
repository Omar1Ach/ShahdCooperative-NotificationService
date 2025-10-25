using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Events;
using Xunit;

namespace ShahdCooperative.NotificationService.Domain.Tests.Events;

public class BaseEventTests
{
    private class TestEvent : BaseEvent { }

    [Fact]
    public void BaseEvent_Should_Initialize_With_Default_Values()
    {
        var ev = new TestEvent();

        ev.EventId.Should().NotBe(Guid.Empty);
        ev.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        ev.EventType.Should().BeEmpty();
    }

    [Fact]
    public void BaseEvent_Should_Generate_Unique_EventIds()
    {
        var event1 = new TestEvent();
        var event2 = new TestEvent();

        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void BaseEvent_Should_Allow_Property_Setting()
    {
        var eventId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var ev = new TestEvent
        {
            EventId = eventId,
            Timestamp = timestamp,
            EventType = "TestEventType"
        };

        ev.EventId.Should().Be(eventId);
        ev.Timestamp.Should().Be(timestamp);
        ev.EventType.Should().Be("TestEventType");
    }
}
