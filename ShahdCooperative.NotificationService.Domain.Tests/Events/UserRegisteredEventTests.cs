using FluentAssertions;
using ShahdCooperative.NotificationService.Domain.Events;
using Xunit;

namespace ShahdCooperative.NotificationService.Domain.Tests.Events;

public class UserRegisteredEventTests
{
    [Fact]
    public void UserRegisteredEvent_Should_Initialize_With_Default_Values()
    {
        var ev = new UserRegisteredEvent();

        ev.UserId.Should().Be(Guid.Empty);
        ev.Email.Should().BeEmpty();
        ev.FullName.Should().BeEmpty();
        ev.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void UserRegisteredEvent_Should_Allow_Property_Setting()
    {
        var userId = Guid.NewGuid();
        var ev = new UserRegisteredEvent
        {
            UserId = userId,
            Email = "test@example.com",
            FullName = "Test User",
            PhoneNumber = "+1234567890"
        };

        ev.UserId.Should().Be(userId);
        ev.Email.Should().Be("test@example.com");
        ev.FullName.Should().Be("Test User");
        ev.PhoneNumber.Should().Be("+1234567890");
    }
}
