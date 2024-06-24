using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class FilterEventModel : FilterItemModel
    {
        public int? EventTypeId { get; set; }

        public int? EventGenreId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
