using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Location
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int LocationTypeId { get; set; }

    public string Address { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Status { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual LocationType LocationType { get; set; } = null!;
}
