using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Event
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid OrganizerId { get; set; }

    public int EventTypeId { get; set; }

    public int EventGenreId { get; set; }

    public Guid LocationId { get; set; }

    public Guid PictureId { get; set; }

    public string Description { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsOutside { get; set; }

    public virtual EventTypeGenre EventNavigation { get; set; } = null!;

    public virtual Location Location { get; set; } = null!;

    public virtual User Organizer { get; set; } = null!;

    public virtual Picture Picture { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();
}
