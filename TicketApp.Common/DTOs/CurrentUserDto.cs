using System;
using System.Collections.Generic;

namespace TicketApp.Common.DTOs
{
    public class CurrentUserDto
    {

        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Role { get; set; }
        public string Subscription { get; set; }
        public bool IsDisabled { get; set; }

    }
}
