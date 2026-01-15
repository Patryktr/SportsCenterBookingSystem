using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Features.Availability.CheckAvailability;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Tests.Integration;

public class CheckAvailabilityHandlerTests : IDisposable
{
    private readonly SportsCenterDbContext _db;
    private readonly CheckAvailabilityHandler _handler;
    private readonly Facility _testFacility;
    private readonly Customer _testCustomer;

    public CheckAvailabilityHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SportsCenterDbContext(options);
        _handler = new CheckAvailabilityHandler(_db);

        _testFacility = new Facility
        {
            Id = 1,
            Name = "Kort tenisowy nr 1",
            SportType = SportType.Tennis,
            MaxPlayers = 4,
            PricePerHour = 80,
            IsActive = true
        };

        _testCustomer = new Customer
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@test.pl"
        };

        _db.Facilities.Add(_testFacility);
        _db.Customers.Add(_testCustomer);
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenSlotAvailable_ShouldReturnIsAvailableTrue()
    {
        // Arrange
        var request = new CheckAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeTrue();
        result.Value.ConflictReason.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSlotBooked_ShouldReturnIsAvailableFalse()
    {
        // Arrange
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            TotalPrice = 160,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var request = new CheckAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(11),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(13)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeFalse();
        result.Value.ConflictReason.Should().Contain("zarezerwowany");
    }

    [Fact]
    public async Task Handle_WhenFacilityNotExists_ShouldReturnIsAvailableFalse()
    {
        // Arrange
        var request = new CheckAvailabilityRequest
        {
            FacilityId = 999,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeFalse();
        result.Value.ConflictReason.Should().Contain("nie istnieje");
    }

    [Fact]
    public async Task Handle_WhenFacilityInactive_ShouldReturnIsAvailableFalse()
    {
        // Arrange
        _testFacility.IsActive = false;
        await _db.SaveChangesAsync();

        var request = new CheckAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeFalse();
        result.Value.ConflictReason.Should().Contain("nieaktywny");
    }

    [Fact]
    public async Task Handle_WhenCanceledBookingExists_ShouldReturnIsAvailableTrue()
    {
        // Arrange
        var canceledBooking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            TotalPrice = 160,
            Status = BookingStatus.Canceled
        };
        _db.Bookings.Add(canceledBooking);
        await _db.SaveChangesAsync();

        var request = new CheckAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBookingDoesNotOverlap_ShouldReturnIsAvailableTrue()
    {
        // Arrange
        var existingBooking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            TotalPrice = 160,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(existingBooking);
        await _db.SaveChangesAsync();

        var request = new CheckAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(16)
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsAvailable.Should().BeTrue();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}