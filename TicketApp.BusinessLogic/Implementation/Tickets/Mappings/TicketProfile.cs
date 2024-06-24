using AutoMapper;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Tickets.Models;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Tickets
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<CreateTicketModel, Ticket>()
                .ForMember(d => d.Id, d => d.MapFrom(s => Guid.NewGuid()));

            CreateMap<Ticket, DisplayTicketModel>()
                .ForMember(d => d.Price, d => d.MapFrom(s => s.Price))
                .ForMember(d => d.Name, d => d.MapFrom(s => s.Name))
                .ForMember(d => d.Description, d => d.MapFrom(s => s.Description));

            CreateMap<Ticket, MyTicketModel>()
                .ForMember(d => d.EventPicture, d => d.MapFrom(s => s.Event.Picture.PictureContent))
                .ForMember(d => d.EventName, d => d.MapFrom(s => s.Event.Name))
                .ForMember(d => d.Location, d => d.MapFrom(s => s.Event.Location.Name))
                .ForMember(d => d.StartDate, d => d.MapFrom(s => s.Event.StartDate))
                .ForMember(d => d.EndDate, d => d.MapFrom(s => s.Event.EndDate));
        }

    }
}
