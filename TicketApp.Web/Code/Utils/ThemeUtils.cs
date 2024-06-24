namespace TicketApp.Web.Code.Utils
{
    public static class ThemeUtils
    {
        public static void UpdateTheme(HttpContext httpContext, string cookieName, string? value)
        {
            httpContext.Session.SetString(cookieName, value);
        }
    }
}
