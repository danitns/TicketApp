using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketApp.BusinessLogic.Implementation.Locations;
using TicketApp.Common.Exceptions;
using TicketApp.Web.Code.Utils;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    public class LocationController : BaseController
    {
        private readonly LocationService Service;
        public LocationController(ControllerDependencies dependencies, LocationService locationService) : base(dependencies)
        {
            Service = locationService;
        }

        public IActionResult PartyMap()
        {
            ThemeUtils.UpdateTheme(HttpContext, "Theme", "dark");
            return View();
        }

        public IActionResult CulturalMap()
        {
            ThemeUtils.UpdateTheme(HttpContext, "Theme", "light");
            return View();
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public IActionResult Create([FromForm] CreateLocationModel model)
        {
            try
            {
                var locationId = Service.CreateLocation(model);
                return Ok(new { id = locationId });
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

        [HttpGet("location-coordinates")]
        public async Task<IActionResult> GetLocationCoordinates(Guid id)
        {
            var coordinates = await Service.GetLocationCoordinates(id);
            return Ok(new { lat = coordinates.Item1, lng = coordinates.Item2 });
        }

        [HttpGet("map-locations")]
        public async Task<ActionResult> GetLocationsForMap(string option, string swLat, string swLng, string neLat, string neLng, 
            string eventTypeFilter, string startDateFilter)
        {
            var locations = await Service.GetLocationsForMap(option, swLat, swLng, neLat, neLng, eventTypeFilter, startDateFilter);
            return Ok(locations);
        }

    }
}
