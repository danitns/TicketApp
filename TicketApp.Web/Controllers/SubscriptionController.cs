using Microsoft.AspNetCore.Mvc;
using TicketApp.BusinessLogic.Implementation.Subscriptions;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    public class SubscriptionController : BaseController
    {
        private readonly SubscriptionService Service;
        public SubscriptionController(ControllerDependencies dependencies, SubscriptionService subscriptionService) : base(dependencies)
        {
            Service = subscriptionService;
        }

        public IActionResult Index()
        {
            var listOfSubscriptions = Service.GetSubscriptions();
            return View(listOfSubscriptions);
        }
    }
}
