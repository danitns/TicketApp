using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketApp.BusinessLogic.Implementation.Events;
using TicketApp.Common.DTOs;
using TicketApp.Entities.Enums;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    public class EventController : BaseController
    {
        private readonly EventService Service;
        public EventController(ControllerDependencies dependencies, EventService eventService) : base(dependencies)
        {
            Service = eventService;
        }

        public async Task<IActionResult> Index(FilterEventModel? filterEventModel = null)
        {
            var listOfEvents = await Service.GetEvents(filterEventModel);
                
            return View(listOfEvents);
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await Service.CreateEventModelAndInitValuesAsync();
            return View("Create", model);
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEventModel model)
        {
            await Service.CreateEvent(model);

            return RedirectToAction("OrganizerPanel", "Event");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var model = await Service.GetEventForDetails(id);

            return View(model);

        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await Service.GetEventForEdit(id);
            return View("Edit", model);
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditEventModel model)
        {
            await Service.EditEvent(model);
            return RedirectToAction("OrganizerPanel", "Event");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Service.DeleteEvent(id);
            return RedirectToAction("Index", "Event");
        }

        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> OrganizerPanel()
        {
            var events = await Service.GetOrganizerEvents();
            return View(events);
        }

        [HttpGet("monthly-transactions")]
        public async Task<IActionResult> MonthlyTransactions()
        {
            var transactions = await Service.GetMonthlyTransactions();
            return Ok(transactions);
        }

        [HttpGet("transactions-by-type")]
        public async Task<IActionResult> TransactionsByEventType()
        {
            var transactionsByEventType = await Service.GetTicketTransactionsByEventType();
            return Ok(transactionsByEventType);
        }

        [HttpGet("corresponding-event-genres")]
        public async Task<ActionResult> GetAvailableEventGenres(string eventTypeId)
        {
            var eventTypes = await Service.GetCorrespondingEventGenres(eventTypeId);
            return Ok(eventTypes);
        }

        [HttpGet("map-eventTypes")]
        public async Task<IActionResult> GetEventTypes(string option)
        {

            var eventTypes = await Service.GetEventTypeValuesForMap(option);
            return Ok(eventTypes);
        }

        [HttpGet("get-filters")]
        public IActionResult GetFiltersAndCurrentPage()
        {
            var filtersAndPagination = Service.GetFiltersAndCurrentPage();
            return Ok(filtersAndPagination);
        }

        [HttpPost("/gpt-response")]
        public async Task<IActionResult> GPTChat([FromBody]string prompt)
        {
            var response = await Service.CreatePromptForChat(prompt);
            return Ok(new { success = response });
        }
    }
}
