using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;
using TicketApp.Entities.Enums;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterModel, User>()
                .ForMember(a => a.Id, a => a.MapFrom(s => Guid.NewGuid()))
                .ForMember(a => a.RoleId, a => a.MapFrom(s => ((int)RoleTypes.User)))
                .ForMember(a => a.PictureId, a => a.Ignore())
                .ForMember(a => a.Picture, a => a.Ignore());

            CreateMap<User, UserDetailsModel>()
                .ForMember(a => a.Picture, a => a.MapFrom(s => s.Picture.PictureContent))
                .ForMember(a => a.Transactions, a => a.Ignore());

            CreateMap<User, PendingOrganizer>()
                .ForMember(a => a.PictureContent, a => a.MapFrom(s => s.Picture.PictureContent));

            CreateMap<User, EditUserModel>()
                .ForMember(a => a.Picture, a => a.MapFrom(s => s.Picture.PictureContent))
                .ForMember(a => a.NewPicture, a => a.Ignore());

            CreateMap<EditUserModel, User>()
                .ForMember(a => a.Picture, a => a.Ignore());
        }
     
    }
}
