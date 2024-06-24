using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class DisplayArtistForIndexModel
    {
        public Guid Id { get; set; }

        public string ArtistTypeName { get; set; }

        public string Name { get; set; }

        public DateTime Birthdate { get; set; }

        public DateTime Debut { get; set; }
    }
}
