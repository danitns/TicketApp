using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public int RoleId { get; set; }

    public Guid? PictureId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? FavouriteGenre { get; set; }

    public bool IsEdited { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual EventGenre? FavouriteGenreNavigation { get; set; }

    public virtual Picture? Picture { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
