using FluentAssertions;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Tests.Unit;

public class BookingCollisionTests
{
    [Theory]
    [InlineData("10:00", "12:00", "11:00", "13:00", true)]   // Nakładanie się z prawej
    [InlineData("10:00", "12:00", "09:00", "11:00", true)]   // Nakładanie się z lewej
    [InlineData("10:00", "12:00", "10:30", "11:30", true)]   // Zawieranie się
    [InlineData("10:00", "12:00", "09:00", "13:00", true)]   // Obejmowanie
    [InlineData("10:00", "12:00", "12:00", "14:00", false)]  // Stykanie się (koniec = początek)
    [InlineData("10:00", "12:00", "08:00", "10:00", false)]  // Stykanie się (koniec = początek)
    [InlineData("10:00", "12:00", "13:00", "15:00", false)]  // Brak nakładania
    [InlineData("10:00", "12:00", "07:00", "09:00", false)]  // Brak nakładania
    public void BookingsOverlap_ShouldDetectCollisionsCorrectly(
        string existing1Start, string existing1End,
        string newStart, string newEnd,
        bool shouldOverlap)
    {
        // Arrange
        var baseDate = new DateTime(2026, 1, 20);
        
        var existingBooking = new Booking
        {
            Id = 1,
            FacilityId = 1,
            Start = baseDate.Add(TimeSpan.Parse(existing1Start)),
            End = baseDate.Add(TimeSpan.Parse(existing1End)),
            Status = BookingStatus.Active
        };

        var newBookingStart = baseDate.Add(TimeSpan.Parse(newStart));
        var newBookingEnd = baseDate.Add(TimeSpan.Parse(newEnd));

        // Act - standardowy algorytm wykrywania nakładania się przedziałów
        var overlaps = existingBooking.Start < newBookingEnd && existingBooking.End > newBookingStart;

        // Assert
        overlaps.Should().Be(shouldOverlap);
    }

    [Fact]
    public void CancelledBooking_ShouldNotCauseCollision()
    {
        // Arrange
        var baseDate = new DateTime(2026, 1, 20);
        
        var cancelledBooking = new Booking
        {
            Id = 1,
            FacilityId = 1,
            Start = baseDate.AddHours(10),
            End = baseDate.AddHours(12),
            Status = BookingStatus.Canceled // Anulowana rezerwacja
        };

        var newBookingStart = baseDate.AddHours(10);
        var newBookingEnd = baseDate.AddHours(12);

        // Act - tylko aktywne rezerwacje powinny być sprawdzane
        var shouldCheck = cancelledBooking.Status == BookingStatus.Active;
        var overlaps = shouldCheck && 
                       cancelledBooking.Start < newBookingEnd && 
                       cancelledBooking.End > newBookingStart;

        // Assert
        overlaps.Should().BeFalse("Anulowane rezerwacje nie powinny blokować terminów");
    }

    [Fact]
    public void DifferentFacilities_ShouldNotCauseCollision()
    {
        // Arrange
        var baseDate = new DateTime(2026, 1, 20);
        
        var existingBooking = new Booking
        {
            Id = 1,
            FacilityId = 1, // Kort 1
            Start = baseDate.AddHours(10),
            End = baseDate.AddHours(12),
            Status = BookingStatus.Active
        };

        var newBookingFacilityId = 2; // Kort 2
        var newBookingStart = baseDate.AddHours(10);
        var newBookingEnd = baseDate.AddHours(12);

        // Act
        var sameFacility = existingBooking.FacilityId == newBookingFacilityId;
        var overlaps = sameFacility && 
                       existingBooking.Start < newBookingEnd && 
                       existingBooking.End > newBookingStart;

        // Assert
        overlaps.Should().BeFalse("Rezerwacje różnych obiektów nie powinny kolidować");
    }

    [Fact]
    public void MultipleBookings_ShouldDetectAnyCollision()
    {
        // Arrange
        var baseDate = new DateTime(2026, 1, 20);
        
        var existingBookings = new List<Booking>
        {
            new() { Id = 1, FacilityId = 1, Start = baseDate.AddHours(8), End = baseDate.AddHours(10), Status = BookingStatus.Active },
            new() { Id = 2, FacilityId = 1, Start = baseDate.AddHours(12), End = baseDate.AddHours(14), Status = BookingStatus.Active },
            new() { Id = 3, FacilityId = 1, Start = baseDate.AddHours(16), End = baseDate.AddHours(18), Status = BookingStatus.Active }
        };

        var newBookingStart = baseDate.AddHours(13); // Koliduje z rezerwacją #2
        var newBookingEnd = baseDate.AddHours(15);

        // Act
        var hasCollision = existingBookings
            .Where(b => b.Status == BookingStatus.Active)
            .Any(b => b.Start < newBookingEnd && b.End > newBookingStart);

        // Assert
        hasCollision.Should().BeTrue();
    }

    [Fact]
    public void ExactSameTimeSlot_ShouldCauseCollision()
    {
        // Arrange
        var baseDate = new DateTime(2026, 1, 20);
        
        var existingBooking = new Booking
        {
            Id = 1,
            FacilityId = 1,
            Start = baseDate.AddHours(10),
            End = baseDate.AddHours(12),
            Status = BookingStatus.Active
        };

        var newBookingStart = baseDate.AddHours(10); // Dokładnie ten sam czas
        var newBookingEnd = baseDate.AddHours(12);

        // Act
        var overlaps = existingBooking.Start < newBookingEnd && existingBooking.End > newBookingStart;

        // Assert
        overlaps.Should().BeTrue("Rezerwacja na dokładnie ten sam termin powinna kolidować");
    }
}