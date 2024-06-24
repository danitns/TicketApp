using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class CreateArtistModel
    {
        public int ArtistTypeId { get; set; }

        public IFormFile Picture { get; set; }

        public string Name { get; set; } = null!;

        public DateTime Birthdate { get; set; }

        public DateTime Debut { get; set; }
    }
}
