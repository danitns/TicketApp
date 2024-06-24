using System.Security.Claims;
using TicketApp.Common.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace TicketApp.Web.Code.Utils
{
    static public class LoginUtils
    {
        public static async Task LogIn(CurrentUserDto user, HttpContext httpContext)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.Name}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Subscription", user.Subscription),
                new Claim("IsDisabled", user.IsDisabled.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                    scheme: "TicketAppCookies",
                    principal: principal);
        }

        public static async Task LogOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(scheme: "TicketAppCookies");
        }


    }
}
