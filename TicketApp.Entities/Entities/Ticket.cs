using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid EventId { get; set; }

    public decimal Price { get; set; }

    public int Notickets { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<TicketTransaction> TicketTransactions { get; set; } = new List<TicketTransaction>();
}
