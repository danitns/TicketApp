using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class EditEventModel
    {
        public Guid Id { get; set; }

        public Guid OrganizerId { get; set; }

        public string Name { get; set; }

        public int EventTypeId { get; set; }

        public int EventGenreId { get; set; }

        public Guid LocationId { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsOutside { get; set; }

        public IFormFile? NewPicture { get; set; }

        public Guid PictureId { get; set; }

        public byte[]? OldPicture { get; set; }

        public List<ListItemModel<string, int>> EventTypes { get; set; } = new List<ListItemModel<string, int>>();

        public List<ListItemModel<string, Guid>> Locations { get; set; } = new List<ListItemModel<string, Guid>>();

        public List<Guid>? ArtistsIds { get; set; }

        public List<MultiSelectListItemModel<string, Guid>> ArtistOptions { get; set; } = new List<MultiSelectListItemModel<string, Guid>>();
    }
}
