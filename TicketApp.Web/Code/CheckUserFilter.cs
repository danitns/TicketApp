using Microsoft.AspNetCore.Mvc.Filters;
using TicketApp.BusinessLogic.Implementation.Account;
using TicketApp.Common.DTOs;
using TicketApp.Web.Code.Utils;

namespace TicketApp.Web.Code
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckUserFilter : ActionFilterAttribute, IFilterMetadata
    {
        private readonly UserAccountService Service;
        private readonly CurrentUserDto CurrentUser;

        public CheckUserFilter(UserAccountService service, CurrentUserDto currentUser)
        {
            Service = service;
            CurrentUser = currentUser;
        }

        public override async void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Method == HttpMethods.Get)
            {
                var needsUpdate = Service.UserNeedsUpdate(CurrentUser);
                if (needsUpdate)
                {
                    LoginUtils.LogOut(filterContext.HttpContext).Wait();
                    var user = await Service.UpdateUserIsEdited(CurrentUser.Id);
                    if (!user.IsDisabled)
                    {
                        LoginUtils.LogIn(user, filterContext.HttpContext).Wait();
                    }
                    else
                    {
                        LoginUtils.LogIn(user, filterContext.HttpContext).Wait();
                        LoginUtils.LogOut(filterContext.HttpContext).Wait();
                        throw new UnauthorizedAccessException();
                    }
                }

            }
            base.OnActionExecuting(filterContext);
        }
    }
}
