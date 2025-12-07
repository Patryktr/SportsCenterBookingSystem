using SportsCenter.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsCenter.Domain.Entities;

public class Facility
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public SportType SportType { get; set; }
    public int MaxPlayers { get; set; }

    public decimal PricePerHour { get; set; }

    public bool IsActive { get; set; } = true;
}