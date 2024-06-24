using System;
using System.Collections.Generic;

namespace TicketApp.Entities;

public partial class TicketTransaction
{
    public Guid Id { get; set; }

    public Guid? TicketId { get; set; }

    public Guid? TransactionId { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual Transaction? Transaction { get; set; }
}
