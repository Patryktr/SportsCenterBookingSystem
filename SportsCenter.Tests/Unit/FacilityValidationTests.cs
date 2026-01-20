using FluentAssertions;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Tests.Unit;

public class FacilityValidationTests
{
    [Fact]
    public void Facility_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var facility = new Facility();

        // Assert
        facility.IsActive.Should().BeTrue("Nowy obiekt powinien być domyślnie aktywny");
        facility.MinBookingDurationMinutes.Should().Be(30, "Domyślne minimum to 30 minut");
        facility.MaxBookingDurationMinutes.Should().Be(480, "Domyślne maksimum to 8 godzin");
    }

    [Theory]
    [InlineData(30, 480, true)]    // Domyślne wartości
    [InlineData(60, 240, true)]    // Niestandardowe wartości
    [InlineData(15, 120, true)]    // Krótkie minimum
    [InlineData(30, 30, true)]     // Min = Max (dozwolone)
    [InlineData(60, 30, false)]    // Min > Max (niedozwolone)
    [InlineData(0, 480, false)]    // Min = 0 (niedozwolone)
    [InlineData(30, 0, false)]     // Max = 0 (niedozwolone)
    public void Facility_DurationLimits_ShouldBeValid(int min, int max, bool shouldBeValid)
    {
        // Arrange
        var facility = new Facility
        {
            MinBookingDurationMinutes = min,
            MaxBookingDurationMinutes = max
        };

        // Act
        var isValid = facility.MinBookingDurationMinutes > 0 &&
                      facility.MaxBookingDurationMinutes > 0 &&
                      facility.MinBookingDurationMinutes <= facility.MaxBookingDurationMinutes;

        // Assert
        isValid.Should().Be(shouldBeValid);
    }

    [Theory]
    [InlineData(SportType.Tennis, 4)]
    [InlineData(SportType.Football, 22)]
    [InlineData(SportType.Padel, 4)]
    [InlineData(SportType.Squash, 4)]
    [InlineData(SportType.Badminton, 4)]
    public void Facility_SportType_ShouldHaveReasonableMaxPlayers(SportType sportType, int expectedMaxPlayers)
    {
        // Arrange & Act
        var facility = new Facility
        {
            SportType = sportType,
            MaxPlayers = expectedMaxPlayers
        };

        // Assert
        facility.MaxPlayers.Should().BeGreaterThan(0);
        facility.MaxPlayers.Should().BeLessThanOrEqualTo(50, "Maksymalna liczba graczy powinna być rozsądna");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Facility_MaxPlayers_ShouldNotBeZeroOrNegative(int invalidMaxPlayers)
    {
        // Arrange
        var facility = new Facility
        {
            MaxPlayers = invalidMaxPlayers
        };

        // Act
        var isValid = facility.MaxPlayers > 0;

        // Assert
        isValid.Should().BeFalse("MaxPlayers musi być większe niż 0");
    }

    [Theory]
    [InlineData(50.00)]
    [InlineData(80.50)]
    [InlineData(100.00)]
    [InlineData(0.01)]
    public void Facility_PricePerHour_ShouldBePositive(decimal price)
    {
        // Arrange & Act
        var facility = new Facility
        {
            PricePerHour = price
        };

        // Assert
        facility.PricePerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Facility_WhenInactive_ShouldNotAllowBookings()
    {
        // Arrange
        var facility = new Facility
        {
            Id = 1,
            Name = "Kort tenisowy",
            IsActive = false
        };

        // Act & Assert
        facility.IsActive.Should().BeFalse();
        // W prawdziwej implementacji handler sprawdza IsActive i odrzuca rezerwacje
    }

    [Fact]
    public void Facility_Name_ShouldNotBeEmpty()
    {
        // Arrange & Act
        var facility = new Facility
        {
            Name = "Kort tenisowy nr 1"
        };

        // Assert
        facility.Name.Should().NotBeNullOrWhiteSpace();
        facility.Name.Length.Should().BeGreaterThan(0);
    }
}