using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class DisplayArtistInEventModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public byte[] Picture { get; set; }
    }
}
