using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class MapEventModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string EventTypeName { get; set; }

        public byte[] PictureContent { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
