using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Subscriptions
{
    public class UserSubscriptionModel
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Discount { get; set; }

        public DateTime ProcessingDate { get; set; }

        public string Status { get; set; }
    }
}
