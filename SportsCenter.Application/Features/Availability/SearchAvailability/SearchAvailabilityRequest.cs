namespace SportsCenter.Application.Features.Availability.SearchAvailability;

public class SearchAvailabilityRequest
{
    // ID obiektu sportowego (wymagane)
    public int FacilityId { get; set; }
    
    // Data dla której szukamy dostępnych slotów
    public DateTime Date { get; set; }
}