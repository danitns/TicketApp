using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.Common.DTOs
{
    public class GPTRequestModel
    {
        public string Prompt { get; set; }

        public bool IsFavourite { get; set; }
    }
}
