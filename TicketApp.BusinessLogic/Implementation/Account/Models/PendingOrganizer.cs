using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class PendingOrganizer
    {
        public Guid Id { get; set; }

        public byte[] PictureContent { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime Birthdate { get; set; }
    }
}
