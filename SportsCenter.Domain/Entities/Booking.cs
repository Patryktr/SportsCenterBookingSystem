using SportsCenter.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsCenter.Domain.Entities;

    public class Booking
    {
        public int Id { get; set; }


    public int FacilityId { get; set; }

        public int CustomerId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int PlayersCount { get; set; }

        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Active;
        public BookingType Type { get; set; } = BookingType.Exclusive;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public Facility Facility { get; set; } = default!;

        public Customer Customer { get; set; } = default!;
    }
