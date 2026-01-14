using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SportsCenter.Application.Features.Bookings.CancelBooking;
using SportsCenter.Domain.Entities;
using SportsCenter.Domain.Entities.Enums;
using SportsCenter.Infrastructure.Persistence;

namespace SportsCenter.Tests.Integration;

public class CancelBookingHandlerTests : IDisposable
{
    private readonly SportsCenterDbContext _db;
    private readonly CancelBookingHandler _handler;
    private readonly Customer _testCustomer;
    private readonly Facility _testFacility;

    public CancelBookingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SportsCenterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SportsCenterDbContext(options);
        
        var loggerMock = new Mock<ILogger<CancelBookingHandler>>();
        _handler = new CancelBookingHandler(_db, loggerMock.Object);

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
            IsActive = true
        };

        _db.Customers.Add(_testCustomer);
        _db.Facilities.Add(_testFacility);
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidBookingMoreThanOneHourAhead_ShouldCancel()
    {
        // Arrange
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddHours(3), // 3 hours from now
            End = DateTime.UtcNow.AddHours(5),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(booking.Id, CancellationToken.None);

        // Assert
        result.Result.Should().Be(CancellationResult.Success);
        
        var updatedBooking = await _db.Bookings.FindAsync(booking.Id);
        updatedBooking!.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task Handle_WithBookingLessThanOneHourAhead_ShouldReturnTooLate()
    {
        // Arrange
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddMinutes(30), // Only 30 minutes from now
            End = DateTime.UtcNow.AddMinutes(30).AddHours(2),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(booking.Id, CancellationToken.None);

        // Assert
        result.Result.Should().Be(CancellationResult.TooLateToCancel);
        
        var unchangedBooking = await _db.Bookings.FindAsync(booking.Id);
        unchangedBooking!.Status.Should().Be(BookingStatus.Active);
    }

    [Fact]
    public async Task Handle_WithNonExistentBooking_ShouldReturnNotFound()
    {
        // Act
        var result = await _handler.Handle(999, CancellationToken.None);

        // Assert
        result.Result.Should().Be(CancellationResult.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyCancelledBooking_ShouldReturnAlreadyCancelled_Idempotent()
    {
        // Arrange
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddHours(3),
            End = DateTime.UtcNow.AddHours(5),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Canceled // Already cancelled
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(booking.Id, CancellationToken.None);

        // Assert
        result.Result.Should().Be(CancellationResult.AlreadyCancelled);
    }

    [Fact]
    public async Task Handle_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddHours(3),
            End = DateTime.UtcNow.AddHours(5),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Act - first call
        var firstResult = await _handler.Handle(booking.Id, CancellationToken.None);
        
        // Act - second call (idempotent)
        var secondResult = await _handler.Handle(booking.Id, CancellationToken.None);

        // Assert
        firstResult.Result.Should().Be(CancellationResult.Success);
        secondResult.Result.Should().Be(CancellationResult.AlreadyCancelled);
        
        // Final state should still be cancelled
        var finalBooking = await _db.Bookings.FindAsync(booking.Id);
        finalBooking!.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task Handle_WithExactlyOneHourBefore_ShouldCancel()
    {
        // Arrange - exactly 1 hour before should still be allowed
        var booking = new Booking
        {
            FacilityId = _testFacility.Id,
            CustomerId = _testCustomer.Id,
            Start = DateTime.UtcNow.AddHours(1).AddMinutes(1), // Just over 1 hour
            End = DateTime.UtcNow.AddHours(3),
            PlayersCount = 2,
            TotalPrice = 100,
            Status = BookingStatus.Active
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(booking.Id, CancellationToken.None);

        // Assert
        result.Result.Should().Be(CancellationResult.Success);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}