using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class EventGenre
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<EventTypeGenre> EventTypeGenres { get; set; } = new List<EventTypeGenre>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
