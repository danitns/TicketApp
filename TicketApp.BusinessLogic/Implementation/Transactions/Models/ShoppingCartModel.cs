using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Tickets.Models;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Transactions
{
    public class ShoppingCartModel
    {
        public Guid Id { get; set; }

        public List<ShoppingCartTicket> AddedTickets { get; set; } = new List<ShoppingCartTicket>();

        public Guid? SubscriptionId { get; set; }

        public int Discount { get; set; } = 0;
    }
}
