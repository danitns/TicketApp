using Hangfire.Dashboard;
using System.Security.Claims;
using TicketApp.Entities.Enums;

namespace TicketApp.Web.Code
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var claims = httpContext.User.Claims;
            var roleClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return roleClaim == RoleTypes.Admin.ToString();
        }
    }
}
