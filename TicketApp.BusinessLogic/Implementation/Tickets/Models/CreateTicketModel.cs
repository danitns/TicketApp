using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Tickets
{
    public class CreateTicketModel
    {
        public string Name { get; set; }

        public Guid EventId { get; set; }

        public decimal Price { get; set; }

        public int Notickets { get; set; }

        public string Description { get; set; }
    }
}
