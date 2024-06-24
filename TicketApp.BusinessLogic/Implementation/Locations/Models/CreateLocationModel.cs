using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class CreateLocationModel
    {
        public string Name { get; set; }

        public int LocationTypeId { get; set; }

        public string Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
