using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.Common.DTOs;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Events
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, ListEventsModel>()
                .ForMember(d => d.LocationName, d => d.MapFrom(s => s.Location.Name))
                .ForMember(d => d.PictureContent, d => d.MapFrom(s => s.Picture.PictureContent));

            CreateMap<Event, StatisticEventModel>()
                .ForMember(d => d.LocationName, d => d.MapFrom(s => s.Location.Name))
                .ForMember(d => d.PictureContent, d => d.MapFrom(s => s.Picture.PictureContent))
                .ForMember(d => d.NoSales, d => d.MapFrom(s => s.Tickets
                    .Sum(t => t.TicketTransactions
                        .Where(tt => tt.Transaction.ProcessingDate != null)
                        .Count()
                        )));

            CreateMap<CreateEventModel, Event>()
                .ForMember(d => d.Id, d => d.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.OrganizerId, d => d.Ignore())
                .ForMember(d => d.Organizer, d => d.Ignore())
                .ForMember(d => d.PictureId, d => d.Ignore())
                .ForMember(d => d.Picture, d => d.Ignore())
                .ForMember(d => d.Artists, d => d.Ignore());

            CreateMap<Event, DetailsEventModel>()
                .ForMember(d => d.EventTypeName, d => d.MapFrom(s => s.EventNavigation.EventType.Name))
                .ForMember(d => d.EventGenreName, d => d.MapFrom(s => s.EventNavigation.EventGenre.Name))
                .ForMember(d => d.PictureContent, d => d.MapFrom(s => s.Picture.PictureContent))
                .ForMember(d => d.IsMyEvent, d => d.Ignore());


            CreateMap<Event, EditEventModel>()
                .ForMember(d => d.NewPicture, d => d.Ignore())
                .ForMember(d => d.OldPicture, d => d.MapFrom(s => s.Picture.PictureContent))
                .ForMember(d => d.EventTypes, d => d.Ignore())
                .ForMember(d => d.Locations, d => d.Ignore())
                .ForMember(d => d.ArtistOptions, d => d.Ignore())
                .ForMember(d => d.ArtistsIds, d => d.MapFrom(
                    s => s.Artists.Select(a => a.Id).ToList()
                    ));

            CreateMap<EditEventModel, Event>()
                .ForMember(d => d.Artists, d => d.Ignore());


            CreateMap<Event, MapEventModel>()
                .ForMember(d => d.EventTypeName, d => d.MapFrom(s => s.EventNavigation.EventType.Name))
                .ForMember(d => d.PictureContent, d => d.MapFrom(s => s.Picture.PictureContent));
        }
    }
}
