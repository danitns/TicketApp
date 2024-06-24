using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class StatisticEventModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public string LocationName { get; set; }

        public byte[] PictureContent { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int NoSales { get; set; }
    }
}
