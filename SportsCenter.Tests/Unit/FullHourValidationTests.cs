using FluentAssertions;

namespace SportsCenter.Tests.Unit;

public class FullHourValidationTests
{
    [Theory]
    [InlineData(10, 0, 0, true)]      // 10:00:00 - pełna godzina
    [InlineData(14, 0, 0, true)]      // 14:00:00 - pełna godzina
    [InlineData(0, 0, 0, true)]       // 00:00:00 - pełna godzina (północ)
    [InlineData(23, 0, 0, true)]      // 23:00:00 - pełna godzina
    [InlineData(10, 30, 0, false)]    // 10:30:00 - połowa godziny
    [InlineData(10, 15, 0, false)]    // 10:15:00 - kwadrans
    [InlineData(10, 45, 0, false)]    // 10:45:00 - kwadrans
    [InlineData(10, 0, 30, false)]    // 10:00:30 - z sekundami
    [InlineData(10, 1, 0, false)]     // 10:01:00 - jedna minuta
    public void IsFullHour_ShouldValidateCorrectly(int hour, int minute, int second, bool expectedResult)
    {
        // Arrange
        var dateTime = new DateTime(2026, 1, 25, hour, minute, second);

        // Act
        var isFullHour = dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;

        // Assert
        isFullHour.Should().Be(expectedResult);
    }

    [Fact]
    public void BookingTimes_WhenBothFullHours_ShouldBeValid()
    {
        // Arrange
        var start = new DateTime(2026, 1, 25, 10, 0, 0);
        var end = new DateTime(2026, 1, 25, 12, 0, 0);

        // Act
        var startIsFullHour = start.Minute == 0 && start.Second == 0;
        var endIsFullHour = end.Minute == 0 && end.Second == 0;

        // Assert
        startIsFullHour.Should().BeTrue();
        endIsFullHour.Should().BeTrue();
    }

    [Fact]
    public void BookingTimes_WhenStartNotFullHour_ShouldBeInvalid()
    {
        // Arrange
        var start = new DateTime(2026, 1, 25, 10, 30, 0); // 10:30
        var end = new DateTime(2026, 1, 25, 12, 0, 0);    // 12:00

        // Act
        var startIsFullHour = start.Minute == 0 && start.Second == 0;

        // Assert
        startIsFullHour.Should().BeFalse("Rezerwacja o 10:30 nie jest pełną godziną");
    }

    [Fact]
    public void BookingTimes_WhenEndNotFullHour_ShouldBeInvalid()
    {
        // Arrange
        var start = new DateTime(2026, 1, 25, 10, 0, 0);  // 10:00
        var end = new DateTime(2026, 1, 25, 11, 45, 0);   // 11:45

        // Act
        var endIsFullHour = end.Minute == 0 && end.Second == 0;

        // Assert
        endIsFullHour.Should().BeFalse("Rezerwacja kończąca się o 11:45 nie jest pełną godziną");
    }

    [Theory]
    [InlineData("2026-01-25T10:00:00", true)]
    [InlineData("2026-01-25T10:00:00.000", true)]
    [InlineData("2026-01-25T10:30:00", false)]
    [InlineData("2026-01-25T10:00:01", false)]
    [InlineData("2026-01-25T10:00:00.001", false)]
    public void IsFullHour_WithDateTimeString_ShouldValidateCorrectly(string dateTimeStr, bool expectedResult)
    {
        // Arrange
        var dateTime = DateTime.Parse(dateTimeStr);

        // Act
        var isFullHour = dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;

        // Assert
        isFullHour.Should().Be(expectedResult);
    }
}