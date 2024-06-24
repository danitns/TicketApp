﻿using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class EventType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsNightEventType { get; set; }

    public virtual ICollection<EventTypeGenre> EventTypeGenres { get; set; } = new List<EventTypeGenre>();
}
