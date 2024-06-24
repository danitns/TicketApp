using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketApp.BusinessLogic.Implementation.Account;
using TicketApp.Common.DTOs;
using TicketApp.Entities;
using TicketApp.WebApp.Code.Base;
using TicketApp.Web.Code.Utils;

namespace TicketApp.Web.Controllers
{
    public class UserAccountController : BaseController
    {
        private readonly UserAccountService Service;

        public UserAccountController(ControllerDependencies dependencies, UserAccountService service)
            : base(dependencies)
        {
            Service = service;
        }


        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterModel();

            return View("Register", model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = await Service.RegisterNewUser(model);
            await LoginUtils.LogIn(user, HttpContext);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Details()
        {
            var model = await Service.AccountDetails();

            if (model == null)
            {
                return View("Error_NotFound");
            }

            return View(model);

        }

        [Authorize]
        public async Task<IActionResult> Delete(Guid? id = null)
        {
            await Service.DeleteAccount(id);

            if (id == null)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("IndexUsers", "Admin");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id = null)
        {
            var model = await Service.GetUserForEdit(id);
            return View("Edit", model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserModel model)
        {
            await Service.EditUser(model);
            if (model.Email == CurrentUser.Email)
                return RedirectToAction("Details", "UserAccount");
            return RedirectToAction("IndexUsers", "Admin");
        }


        [HttpGet]
        public IActionResult Login()
        {
            if(CurrentUser.IsAuthenticated)
            {
                throw new AccessViolationException();
            }
            var model = new LoginModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await Service.Login(model.Email, model.Password);

            if (!user.IsAuthenticated)
            {
                model.AreCredentialsInvalid = true;
                return View(model);
            }

            await LoginUtils.LogIn(user, HttpContext);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await LoginUtils.LogOut(HttpContext);

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> BecomeOrganizer()
        {
            await Service.BecomeOrganizer();
            return Ok(new { success = true });
        }


    }
}
