using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class ListEventsModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public string LocationName { get; set; }

        public byte[] PictureContent { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
