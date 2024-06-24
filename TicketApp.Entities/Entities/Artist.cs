using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Artist
{
    public Guid Id { get; set; }

    public int ArtistTypeId { get; set; }

    public Guid PictureId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public DateTime Debut { get; set; }

    public int Status { get; set; }

    public virtual ArtistType ArtistType { get; set; } = null!;

    public virtual Picture Picture { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
