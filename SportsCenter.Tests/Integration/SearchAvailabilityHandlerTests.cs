using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Features.Availability.SearchAvailability;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Tests.Integration;

public class SearchAvailabilityHandlerTests : IDisposable
{
    private readonly SportsCenterDbContext _db;
    private readonly SearchAvailabilityHandler _handler;
    private readonly Facility _testFacility;

    public SearchAvailabilityHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SportsCenterDbContext(options);
        _handler = new SearchAvailabilityHandler(_db);

        _testFacility = new Facility
        {
            Id = 1,
            Name = "Kort tenisowy nr 1",
            SportType = SportType.Tennis,
            MaxPlayers = 4,
            PricePerHour = 80,
            IsActive = true
        };

        _db.Facilities.Add(_testFacility);
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSlots()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FacilityId.Should().Be(_testFacility.Id);
        result.Value.FacilityName.Should().Be(_testFacility.Name);
        result.Value.Slots.Should().NotBeEmpty();
        result.Value.TotalSlots.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithNonExistentFacility_ShouldReturnFailure()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = 999,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("nie istnieje");
    }

    [Fact]
    public async Task Handle_WithInactiveFacility_ShouldReturnFailure()
    {
        // Arrange
        _testFacility.IsActive = false;
        await _db.SaveChangesAsync();

        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("nieaktywny");
    }

    [Fact]
    public async Task Handle_WithExistingBooking_ShouldMarkSlotAsBooked()
    {
        // Arrange
        var searchDate = DateTime.UtcNow.AddDays(1).Date;
        
        var customer = new Customer
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@test.pl"
        };
        _db.Customers.Add(customer);

        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = customer.Id,
            Start = searchDate.AddHours(10),
            End = searchDate.AddHours(12),
            PlayersCount = 2,
            TotalPrice = 160,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = searchDate
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.BookedSlots.Should().BeGreaterThan(0);
        
        var slot10to11 = result.Value.Slots.FirstOrDefault(s => s.StartTime == "10:00");
        var slot11to12 = result.Value.Slots.FirstOrDefault(s => s.StartTime == "11:00");
        
        slot10to11?.Status.Should().Be(TimeSlotStatus.Booked);
        slot11to12?.Status.Should().Be(TimeSlotStatus.Booked);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectMessage_WhenAllAvailable()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(7).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Contain("wolnych terminów");
    }

    [Fact]
    public async Task Handle_Slots_ShouldHaveCorrectTimeFormat()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var slot in result.Value!.Slots)
        {
            slot.StartTime.Should().MatchRegex(@"^\d{2}:00$");
            slot.EndTime.Should().MatchRegex(@"^\d{2}:00$");
            slot.StatusName.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Handle_Slots_ShouldBeInChronologicalOrder()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var slots = result.Value!.Slots;
        
        for (int i = 1; i < slots.Count; i++)
        {
            slots[i].Start.Should().BeAfter(slots[i - 1].Start);
        }
    }

    [Fact]
    public async Task Handle_EachSlot_ShouldBeExactlyOneHour()
    {
        // Arrange
        var request = new SearchAvailabilityRequest
        {
            FacilityId = _testFacility.Id,
            Date = DateTime.UtcNow.AddDays(1).Date
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (var slot in result.Value!.Slots)
        {
            var duration = slot.End - slot.Start;
            duration.TotalHours.Should().Be(1);
        }
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}