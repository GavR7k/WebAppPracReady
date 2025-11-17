using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using WebAppPrac.Data;
using WebAppPrac.Models;

namespace WebAppPrac.Controllers
{

    public class AdminController : Controller
    {

        private readonly IStringLocalizer<AdminController> _localizer;
        private readonly AppDbContext _db;
        public AdminController(IStringLocalizer<AdminController> localizer, AppDbContext db)
        {
            _db = db;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult ShowFormForAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin == "true")
            {
                return View("LeaveAdminMode");
            }

            return View("FormForAdmin");
        }
      

        [HttpPost]
        public IActionResult UnLogin()
        {
            HttpContext.Session.Remove("IsAdmin");
            return View("FormForAdmin");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {


            var user = await _db.AdminUsers
           .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                //реализовать установку что мы в режиме администратора, добавить в навбар статус по типу админ права)
                return RedirectToAction("ShowMainPage", "MainPage");
            }

            ViewBag.AuthorizationError = _localizer["IncorrectInputForAuthorization"];
            return View("FormForAdmin", "Admin");

        }

        [HttpGet]
        public IActionResult RegistrationForm()
        {
            return View("RegistrFormForAdmin");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(string username, string password)
        {
            var userExist = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username);
            if (userExist == null)
            {
                AdminUser newUser = new AdminUser()
                {
                    Username = username,
                    Password = password
                };
                await _db.AdminUsers.AddAsync(newUser);
                await _db.SaveChangesAsync();
                ViewBag.IsRegistered = _localizer["UserIsSuccessfullyRegistered"];
                //уведомить что аккаунт успешно зарегистрирован 
                return View("FormForAdmin");
            }
            //показать уведомление что аккаунт уже есть в системе
            ViewBag.LoginError = _localizer["UserIsAlreydeRegistered"];
            return View("FormForAdmin");

        }
    }
}
