using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Tickets
{
    public class MyTicketModel
    {
        public string Name { get; set; }

        public string EventName { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public Guid EventId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public byte[] EventPicture { get; set; }

    }
}
