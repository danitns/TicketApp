using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class PendingArtistModel
    {
        public Guid Id { get; set; }

        public int ArtistTypeId { get; set; }

        public byte[] Picture { get; set; }

        public string Name { get; set; }

        public DateTime Birthdate { get; set; }

        public DateTime Debut { get; set; }
    }
}
