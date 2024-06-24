using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Subscriptions
{
    public class SubscriptionProfile : Profile
    {
        public SubscriptionProfile()
        {
            CreateMap<Subscription, IndexSubscriptionModel>();
        }
    }
}
