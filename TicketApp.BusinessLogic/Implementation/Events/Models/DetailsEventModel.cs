using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Artists;
using TicketApp.BusinessLogic.Implementation.Tickets;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class DetailsEventModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        [Display(Name ="Event Type")]
        public string EventTypeName { get; set; }

        [Display(Name = "Event Genre")]
        public string EventGenreName { get; set; }

        public Guid LocationId { get; set; }

        [Display(Name = "Location")]
        public string LocationName { get; set; }

        public byte[] PictureContent { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsOutside { get; set; }

        public bool IsMyEvent { get; set; }

        public List<DisplayTicketModel> Tickets { get; set; } = new List<DisplayTicketModel>();

        public List<DisplayArtistInEventModel> Artists { get; set; } = new List<DisplayArtistInEventModel>();
    }
}
