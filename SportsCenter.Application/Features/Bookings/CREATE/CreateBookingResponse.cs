using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportsCenter.Domain.Entities.Enums;

namespace SportsCenter.Application.Features.Bookings.CREATE;
public class CreateBookingResponse
{
    public int BookingId { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
}

