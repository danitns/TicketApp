using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using TicketApp.BusinessLogic.Implementation.Tickets.Models;
using TicketApp.BusinessLogic.Implementation.Transactions;
using TicketApp.Web.Code.Utils;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly TransactionService Service;
        public TransactionController(ControllerDependencies dependencies, TransactionService transactionService) : base(dependencies)
        {
            Service = transactionService;
        }

        [Authorize]
        public async Task<IActionResult> ShoppingCart()
        {
            var shoppingCart = await Service.DisplayShoppingCart();
            return View(shoppingCart);
        }

        public async Task<IActionResult> CompleteFreeGift()
        {
            //await Service.CompleteFreeGiftTransaction();

            return RedirectToAction("MyTickets", "Ticket");
        }

        [Authorize]
        public async Task<IActionResult> AddTicketsInCart([FromForm]List<ShoppingCartTicket> tickets)
        {
            var stockErrors = await Service.CheckStockForTickets(tickets);
            if (stockErrors != null)
            {
                return Ok(new { errors = stockErrors });
            }
            await Service.AddTicketsToCart(tickets);

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription()
        {
            var result = await Service.CancelSubscription();
            return Ok(new { success = result });
        }

        public async Task<IActionResult> ValidateTicket (Guid ticketTransactionId)
        {
            var result = await Service.ValidateTicketTransaction(ticketTransactionId);

            return View(result);
        }

        [Authorize]
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> Checkout()
        {
            var stockErrors = await Service.CheckStockForCheckout();
            if (stockErrors != null)
                return Ok(new { errors = stockErrors });

            var sessionUrl = await Service.BuyTickests();


            return Ok(new { url = sessionUrl });
        }

        [Authorize]
        [HttpPost("update-cart")]
        public async Task<IActionResult> UpdateCart([FromForm]List<ShoppingCartTicket> productsIdsInput)
        {
            var stockErrors = await Service.UpdateShoppingCart(productsIdsInput);

            if (stockErrors != null)
                return Ok(new { errors = stockErrors });

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpPost("buy-subscription")]
        public async Task<IActionResult> BuySubscription([FromForm] string subscriptionId)
        {
            var activeSubScription = await Service.CheckForSubscription();

            if (activeSubScription == true)
                throw new AccessViolationException();

            var session = await Service.BuySubscription(subscriptionId);

            return Redirect(session.Url);
        }

        [Authorize]
        [HttpGet("/get-user-subscription")]
        public async Task<IActionResult> GetUserSubscription()
        {
            var subscription = await Service.GetUserSubscriptionByEmail(CurrentUser.Email);
            if (subscription == null)
            {
                return Ok(new { noSubscription = true });
            }
            return Ok(subscription);
        }

        [HttpGet("/available-gift")]
        public async Task<IActionResult> CheckAvailableGift()
        {
            var result = await Service.CheckAvailableGift();
            return Ok(new { success = result });
        }

        [HttpGet("/free-gift")]
        public async Task<IActionResult> GetAFreeGift()
        {
            var url = await Service.GenerateGift();
            return Ok(new { success = url });
        }

        [Route("webhook")]
        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                
                if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    await Service.HandleChargeSucceeded(charge);
                }
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    if(session.AmountTotal == 0)
                    {
                        await Service.CompleteFreeGiftTransaction(session);
                    }
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}
