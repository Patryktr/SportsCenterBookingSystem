namespace SportsCenter.Domain.Entities.Enums;

public enum TimeSlotStatus
{
    Available = 1,      // Wolne
    Booked = 2,         // Zarezerwowane
    Blocked = 3,        // Zablokowane (przerwa techniczna, wydarzenie)
    Closed = 4,         // Zamknięte (poza godzinami otwarcia)
    Past = 5            // Minione (w przeszłości)
}