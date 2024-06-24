using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketApp.Entities.Enums
{
    public enum RoleTypes: int
    {
        User = 1,
        Organizer = 2,
        Admin = 3,
        PendingOrganizer = 4
    }
}
