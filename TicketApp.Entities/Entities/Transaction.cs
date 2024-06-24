using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? SubscriptionId { get; set; }

    public DateTime? ProcessingDate { get; set; }

    public bool? IsFree { get; set; }

    public virtual Subscription? Subscription { get; set; }

    public virtual ICollection<TicketTransaction> TicketTransactions { get; set; } = new List<TicketTransaction>();

    public virtual User User { get; set; } = null!;
}
