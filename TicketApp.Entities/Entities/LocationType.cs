﻿using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class LocationType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
