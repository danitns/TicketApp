using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class LocationMapModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LocationTypeName { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public List<MapEventModel> Events { get; set; } = new List<MapEventModel>();
    }
}
