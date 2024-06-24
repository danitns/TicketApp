using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.Common.DTOs
{
    public class MultiSelectListItemModel<TText, TValue>
    {
        public TText text { get; set; }
        public TValue value { get; set; }
        public bool selected { get; set; }
    }
}
