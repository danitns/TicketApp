using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Artists
{
    public class ArtistProfile : Profile
    {
        public ArtistProfile()
        {
            CreateMap<CreateArtistModel, Artist>()
                .ForMember(d => d.Id, d => d.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.Picture, d => d.Ignore())
                .ForMember(d => d.PictureId, d => d.Ignore())
                .ForMember(d => d.Status, d => d.MapFrom(s => StatusCode.Pending));

            CreateMap<Artist, DisplayArtistInEventModel>()
                .ForMember(d => d.Picture, d => d.MapFrom(s => s.Picture.PictureContent));

            CreateMap<Artist, DisplayArtistForIndexModel>()
                .ForMember(d => d.ArtistTypeName, d => d.MapFrom(s => s.ArtistType.Name));

            CreateMap<Artist, PendingArtistModel>()
                .ForMember(d => d.Picture, d => d.MapFrom(s => s.Picture.PictureContent));
        }
    }
}
