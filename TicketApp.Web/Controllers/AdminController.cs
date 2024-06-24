using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketApp.BusinessLogic.Implementation.Admin;
using TicketApp.WebApp.Code.Base;

namespace TicketApp.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly AdminService Service;
        public AdminController(ControllerDependencies dependencies, AdminService adminService) : base(dependencies)
        {
            Service = adminService;
        }

        public async Task<IActionResult> IndexUsers()
        {
            var userList = await Service.GetUserList();
            return View(userList);
        }

        public async Task<IActionResult> PendingArtists()
        {
            var pendingArtists = await Service.GetPendingArtists();
            return View(pendingArtists);
        }

        [HttpPost("reject-artist")]
        public async Task<IActionResult> RejectArtist(Guid id)
        {
            await Service.RejectArtist(id);
            return Ok(new { success = true });
        }

        [HttpPost("approve-artist")]
        public async Task<IActionResult> ApproveArtist(Guid id)
        {
            await Service.ApproveArtist(id);
            return Ok(new { success = true });
        }

        public async Task<IActionResult> PendingLocations()
        {
            var pendingArtists = await Service.GetPendingLocations();
            return View(pendingArtists);
        }

        [HttpPost("reject-location")]
        public async Task<IActionResult> RejectLocation(Guid id)
        {
            await Service.RejectLocation(id);
            return Ok(new { success = true });
        }

        [HttpPost("approve-location")]
        public async Task<IActionResult> ApproveLocation(Guid id)
        {
            await Service.ApproveLocation(id);
            return Ok(new { success = true });
        }

        public async Task<IActionResult> PendingOrganizers()
        {
            var pendingOrganizers = await Service.GetPendingOrganizers();
            return View(pendingOrganizers);
        }

        [HttpPost("reject-organizer")]
        public async Task<IActionResult> RejectOrganizer(Guid id)
        {
            await Service.RejectOrganizer(id);
            return Ok(new { success = true });
        }

        [HttpPost("approve-organizer")]
        public async Task<IActionResult> ApproveOrganizer(Guid id)
        {
            await Service.ApproveOrganizer(id);
            return Ok(new { success = true });
        }
    }
}
