using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SportsCenter.Application.Features.Bookings.CreateBooking;
using SportsCenter.Application.Services;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Tests.Integration;

public class CreateBookingHandlerTests : IDisposable
{
    private readonly SportsCenterDbContext _db;
    private readonly CreateBookingHandler _handler;
    private readonly Customer _testCustomer;
    private readonly Facility _testFacility;

    public CreateBookingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SportsCenterDbContext(options);
        
        var availabilityService = new AvailabilityService(_db);
        _handler = new CreateBookingHandler(_db, availabilityService);

        // Setup test data
        _testCustomer = new Customer
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@test.pl"
        };

        _testFacility = new Facility
        {
            Id = 1,
            Name = "Kort tenisowy",
            SportType = SportType.Tennis,
            MaxPlayers = 4,
            PricePerHour = 50,
            MinBookingDurationMinutes = 30,
            MaxBookingDurationMinutes = 480,
            IsActive = true
        };

        _db.Customers.Add(_testCustomer);
        _db.Facilities.Add(_testFacility);
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateBooking()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalPrice.Should().Be(100); // 2h * 50 zł
    }

    [Fact]
    public async Task Handle_WithConflictingBooking_ShouldReturnFailure()
    {
        // Arrange - create existing booking
        var existingBooking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(existingBooking);
        await _db.SaveChangesAsync();

        var conflictingRequest = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(11), // Overlaps with existing
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(13),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(conflictingRequest, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("zarezerwowany");
    }

    [Fact]
    public async Task Handle_WithDurationBelowMinimum_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(15), // Only 15 minutes
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Minimalna długość");
    }

    [Fact]
    public async Task Handle_WithDurationAboveMaximum_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(20), // 12 hours > 8 hours max
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Maksymalna długość");
    }

    [Fact]
    public async Task Handle_WithTooManyPlayers_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 10, // Max is 4
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Maksymalna liczba graczy");
    }

    [Fact]
    public async Task Handle_WithInactiveFacility_ShouldReturnFailure()
    {
        // Arrange
        _testFacility.IsActive = false;
        await _db.SaveChangesAsync();

        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("nieaktywny");
    }

    [Fact]
    public async Task Handle_WithStartInPast_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(-1), // Yesterday
            End = DateTime.UtcNow.AddDays(-1).AddHours(2),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("przeszłości");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}