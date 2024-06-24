using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Subscription
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Discount { get; set; }

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
