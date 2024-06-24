using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.Common.DTOs
{
    public class FilterItemModel
    {
        public int CurrentPage { get; set; }

        public int ItemsOnPage { get; set; }
    }
}
