using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Subscriptions
{
    public class SubscriptionService : BaseService
    {
        public SubscriptionService(ServiceDependencies serviceDependencies) : base(serviceDependencies)
        {
        }

        public List<IndexSubscriptionModel> GetSubscriptions()
        {
            var subscriptions = UnitOfWork.Subscriptions
                .Get()
                .Select(s => Mapper.Map<IndexSubscriptionModel>(s))
                .ToList();

            return subscriptions;
        }


    }
}
