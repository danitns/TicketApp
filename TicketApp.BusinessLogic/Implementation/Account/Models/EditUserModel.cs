using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.BusinessLogic.Implementation.Account
{
    public class EditUserModel
    {
        public Guid PictureId { get; set; }
        public byte[]? Picture { get; set; }

        public IFormFile? NewPicture { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
