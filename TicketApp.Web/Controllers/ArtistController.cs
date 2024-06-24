using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketApp.BusinessLogic.Implementation.Artists;
using TicketApp.Common.Exceptions;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    [Authorize(Roles = "Organizer,Admin")]
    public class ArtistController : BaseController
    {
        private readonly ArtistService Service;
        public ArtistController(ControllerDependencies dependencies, ArtistService artistService) : base(dependencies)
        {
            Service = artistService;
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        public IActionResult Create([FromForm]CreateArtistModel model)
        {
            try
            {
                var artistId = Service.CreateArtist(model);
                return Ok(new { id = artistId });
            }
            catch(ValidationErrorException validationError)
            {
                return Ok(new
                {
                    errors = validationError.ValidationResult.Errors
                                                .Select(e => new List<string>() { e.PropertyName, e.ErrorMessage })
                                                .ToList()
                });
            }
       
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var artists = await Service.GetArtists();
            return View(artists);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Service.DeleteArtist(id);
            return RedirectToAction("Index", "Artist");
        }
    }
}
