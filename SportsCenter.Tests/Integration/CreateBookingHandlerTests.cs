using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportsCenter.Application.Features.Bookings.CreateBooking;
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
        _handler = new CreateBookingHandler(_db);

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
        result.Value!.TotalPrice.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithConflictingBooking_ShouldReturnFailure()
    {
        // Arrange
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
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(11),
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
    public async Task Handle_WithStartNotFullHour_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10).AddMinutes(30),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("pełnej godzinie");
    }

    [Fact]
    public async Task Handle_WithEndNotFullHour_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(11).AddMinutes(45),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("pełnej godzinie");
    }

    [Fact]
    public async Task Handle_WithFullHours_ShouldSucceed()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(16),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Start.Minute.Should().Be(0);
        result.Value.End.Minute.Should().Be(0);
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
            PlayersCount = 10,
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
            Start = DateTime.UtcNow.AddDays(-1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(-1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("przeszłości");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = Guid.NewGuid(),
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Klient nie istnieje");
    }

    [Fact]
    public async Task Handle_WithNonExistentFacility_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = 999,
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
        result.Error.Should().Contain("nie istnieje");
    }

    [Fact]
    public async Task Handle_WithZeroPlayers_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 0,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("większa niż 0");
    }

    [Fact]
    public async Task Handle_WithStartAfterEnd_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateBookingRequest
        {
            FacilityId = _testFacility.Id,
            CustomerPublicId = _testCustomer.PublicId,
            Start = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            End = DateTime.UtcNow.AddDays(1).Date.AddHours(12),
            PlayersCount = 2,
            Type = BookingType.Exclusive
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("wcześniejsza niż data zakończenia");
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}