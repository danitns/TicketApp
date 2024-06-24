using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class PendingLocationModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public int LocationTypeId { get; set; }

        public string Address { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
