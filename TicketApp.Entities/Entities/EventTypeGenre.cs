using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class EventTypeGenre
{
    public int EventTypeId { get; set; }

    public int EventGenreId { get; set; }

    public virtual EventGenre EventGenre { get; set; } = null!;

    public virtual EventType EventType { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
