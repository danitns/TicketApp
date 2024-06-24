using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Locations
{
    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<CreateLocationModel, Location>()
                .ForMember(d => d.Id, d => d.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.Status, d => d.MapFrom(s => StatusCode.Pending));

            CreateMap<Location, LocationMapModel>()
                .ForMember(d => d.LocationTypeName, d => d.MapFrom(s => s.LocationType.Name));

            CreateMap<Location, PendingLocationModel>();

        }
    }
}
