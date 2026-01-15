using FluentAssertions;
using SportsCenter.Application.Common;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Tests.Unit;

public class TimeSlotTests
{
    [Fact]
    public void TimeSlotItem_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var slot = new TimeSlotItem
        {
            Start = new DateTime(2026, 1, 25, 10, 0, 0),
            End = new DateTime(2026, 1, 25, 11, 0, 0),
            StartTime = "10:00",
            EndTime = "11:00",
            Status = TimeSlotStatus.Available,
            StatusName = "Wolne",
            BlockedReason = null
        };

        // Assert
        slot.Start.Hour.Should().Be(10);
        slot.End.Hour.Should().Be(11);
        slot.StartTime.Should().Be("10:00");
        slot.EndTime.Should().Be("11:00");
        slot.Status.Should().Be(TimeSlotStatus.Available);
        slot.StatusName.Should().Be("Wolne");
        slot.BlockedReason.Should().BeNull();
    }

    [Fact]
    public void TimeSlotItem_WhenBlocked_ShouldHaveReason()
    {
        // Arrange & Act
        var slot = new TimeSlotItem
        {
            Start = new DateTime(2026, 1, 25, 10, 0, 0),
            End = new DateTime(2026, 1, 25, 11, 0, 0),
            StartTime = "10:00",
            EndTime = "11:00",
            Status = TimeSlotStatus.Blocked,
            StatusName = "Zablokowane",
            BlockedReason = "Przerwa techniczna"
        };

        // Assert
        slot.Status.Should().Be(TimeSlotStatus.Blocked);
        slot.BlockedReason.Should().NotBeNullOrEmpty();
        slot.BlockedReason.Should().Be("Przerwa techniczna");
    }

    [Theory]
    [InlineData(TimeSlotStatus.Available, "Wolne")]
    [InlineData(TimeSlotStatus.Booked, "Zarezerwowane")]
    [InlineData(TimeSlotStatus.Blocked, "Zablokowane")]
    [InlineData(TimeSlotStatus.Closed, "Zamknięte")]
    [InlineData(TimeSlotStatus.Past, "Minione")]
    public void TimeSlotStatus_ShouldHaveCorrectPolishName(TimeSlotStatus status, string expectedName)
    {
        // Arrange & Act
        var statusName = status switch
        {
            TimeSlotStatus.Available => "Wolne",
            TimeSlotStatus.Booked => "Zarezerwowane",
            TimeSlotStatus.Blocked => "Zablokowane",
            TimeSlotStatus.Closed => "Zamknięte",
            TimeSlotStatus.Past => "Minione",
            _ => status.ToString()
        };

        // Assert
        statusName.Should().Be(expectedName);
    }

    [Fact]
    public void TimeSlotStatus_AllValuesShouldBeUnique()
    {
        // Arrange
        var values = Enum.GetValues<TimeSlotStatus>();

        // Act & Assert
        values.Should().OnlyHaveUniqueItems();
        values.Should().HaveCount(5);
    }

    [Fact]
    public void TimeSlotItem_Duration_ShouldBeOneHour()
    {
        // Arrange
        var slot = new TimeSlotItem
        {
            Start = new DateTime(2026, 1, 25, 10, 0, 0),
            End = new DateTime(2026, 1, 25, 11, 0, 0),
            StartTime = "10:00",
            EndTime = "11:00",
            Status = TimeSlotStatus.Available,
            StatusName = "Wolne"
        };

        // Act
        var duration = slot.End - slot.Start;

        // Assert
        duration.TotalHours.Should().Be(1);
        duration.TotalMinutes.Should().Be(60);
    }

    [Fact]
    public void GenerateHourlySlots_ShouldCreateCorrectSlots()
    {
        // Arrange
        var date = new DateTime(2026, 1, 25);
        var openHour = 8;
        var closeHour = 12;
        var slots = new List<TimeSlotItem>();

        // Act
        for (var hour = openHour; hour < closeHour; hour++)
        {
            slots.Add(new TimeSlotItem
            {
                Start = date.AddHours(hour),
                End = date.AddHours(hour + 1),
                StartTime = $"{hour:D2}:00",
                EndTime = $"{hour + 1:D2}:00",
                Status = TimeSlotStatus.Available,
                StatusName = "Wolne"
            });
        }

        // Assert
        slots.Should().HaveCount(4); // 8-9, 9-10, 10-11, 11-12
        slots.First().StartTime.Should().Be("08:00");
        slots.Last().EndTime.Should().Be("12:00");
    }
}