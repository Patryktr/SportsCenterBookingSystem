using FluentAssertions;

namespace SportsCenter.Tests.Unit;

public class FullHourValidationTests
{
    [Theory]
    [InlineData(10, 0, 0, true)]
    [InlineData(14, 0, 0, true)]
    [InlineData(0, 0, 0, true)]
    [InlineData(23, 0, 0, true)]
    [InlineData(10, 30, 0, false)]
    [InlineData(10, 15, 0, false)]
    [InlineData(10, 45, 0, false)]
    [InlineData(10, 0, 30, false)]
    [InlineData(10, 1, 0, false)]
    public void IsFullHour_ShouldValidateCorrectly(int hour, int minute, int second, bool expectedResult)
    {
        // Arrange
        var dateTime = new DateTime(2026, 1, 25, hour, minute, second);

        // Act
        var isFullHour = IsFullHour(dateTime);

        // Assert
        isFullHour.Should().Be(expectedResult);
    }

    [Fact]
    public void BookingTimes_WhenBothFullHours_ShouldBeValid()
    {
        // Arrange
        var start = new DateTime(2026, 1, 25, 10, 0, 0);
        var end = new DateTime(2026, 1, 25, 12, 0, 0);

        // Act & Assert
        IsFullHour(start).Should().BeTrue();
        IsFullHour(end).Should().BeTrue();
    }

    [Fact]
    public void BookingTimes_WhenStartNotFullHour_ShouldBeInvalid()
    {
        // Arrange
        var start = new DateTime(2026, 1, 25, 10, 30, 0);

        // Act & Assert
        IsFullHour(start).Should().BeFalse("Rezerwacja o 10:30 nie jest pełną godziną");
    }

    [Fact]
    public void BookingTimes_WhenEndNotFullHour_ShouldBeInvalid()
    {
        // Arrange
        var end = new DateTime(2026, 1, 25, 11, 45, 0);

        // Act & Assert
        IsFullHour(end).Should().BeFalse("Rezerwacja kończąca się o 11:45 nie jest pełną godziną");
    }

    private static bool IsFullHour(DateTime dateTime)
    {
        return dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }
}