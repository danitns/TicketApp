using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Picture
{
    public Guid Id { get; set; }

    public byte[]? PictureContent { get; set; }

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
