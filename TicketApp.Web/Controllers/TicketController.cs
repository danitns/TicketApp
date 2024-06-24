using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using TicketApp.BusinessLogic.Implementation.Tickets;
using TicketApp.Common.Exceptions;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    public class TicketController : BaseController
    {
        private readonly TicketService Service;

        public TicketController(ControllerDependencies dependencies, TicketService ticketService) : base(dependencies)
        {
            Service = ticketService;
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTicketModel model)
        {
            try
            {
                await Service.CreateTicket(model);
                return Ok(new { success = true });
            }
            catch (ValidationErrorException validationError)
            {
                return Ok(new
                {
                    errors = validationError.ValidationResult.Errors
                                                .Select(e => new List<string>() { e.PropertyName, e.ErrorMessage })
                                                .ToList()
                });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyTickets()
        {
            var myTickets = await Service.GetTicketsForCurrentUser();
            return View(myTickets);
        }

        [HttpGet("get-qr-codes")]
        public async Task<IActionResult> QrCodesForTickets(string ticketName, Guid eventId)
        {
            var qrCodes = await Service.GetQrCodes(ticketName, eventId);
            return Ok(qrCodes);
        }

        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Delete(Guid id, Guid eventId)
        {
            await Service.DeleteTicket(id);
            return RedirectToAction("Details", "Event", new {id = eventId});
        }

        [HttpPost("get-ticket-details")]
        public async Task<IActionResult> GetTicketDetails([FromBody]string[] ticketIds)
        {
            var ticket = await Service.GetTicketDetails(ticketIds);
            return Ok(ticket);
        }
    }
}
