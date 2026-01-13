namespace SportsCenter.Domain.Entities.Enums;

public enum AvailabilityConflictType
{
    None = 0,
    ExistingBooking = 1,
    TimeBlock = 2,
    OutsideOperatingHours = 3,
    FacilityClosed = 4,
    FacilityInactive = 5
}