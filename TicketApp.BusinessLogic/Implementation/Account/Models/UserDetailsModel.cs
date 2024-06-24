using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketApp.BusinessLogic.Implementation.Transactions;
using TicketApp.Entities;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class UserDetailsModel
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime Birthdate { get; set; }

        public byte[] Picture { get; set; }

        public int RoleId { get; set; }

        public List<DisplayTransactionModel> Transactions { get; set; } = new List<DisplayTransactionModel>();
    }
}
