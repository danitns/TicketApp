using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Transactions
{
    public class DisplayTransactionModel
    {
        public DateTime ProcessingDate { get; set; }

        public string? SubscriptionName { get; set; }

        public List<Tuple<string, int>> Tickets { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
