using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Domain.Entities;

public class Facility
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public SportType SportType { get; set; }
    public int MaxPlayers { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Minimalna długość rezerwacji w minutach (domyślnie 30 minut)
    /// </summary>
    public int MinBookingDurationMinutes { get; set; } = 30;
    
    /// <summary>
    /// Maksymalna długość rezerwacji w minutach (domyślnie 480 minut = 8 godzin)
    /// </summary>
    public int MaxBookingDurationMinutes { get; set; } = 480;
}