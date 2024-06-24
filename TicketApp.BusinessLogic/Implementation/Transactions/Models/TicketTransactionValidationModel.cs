using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Transactions
{
    public class TicketTransactionValidationModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid EventId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public byte[] EventPicture { get; set; }
    }
}
