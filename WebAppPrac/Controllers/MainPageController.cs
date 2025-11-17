using Microsoft.AspNetCore.Mvc;
using WebAppPrac.Services;
using WebAppPrac.Controllers;
using System.Globalization;
using WebAppPrac.Data;
using Microsoft.EntityFrameworkCore;

namespace WebAppPrac.Controllers
{
    public class MainPageController : Controller
    {
        private readonly ItranslationService _translator;
        private readonly AppDbContext _dp;

        public MainPageController(ItranslationService translator, AppDbContext db)
        {

            _translator = translator;
            _dp = db;
        }

        public async Task<IActionResult> ShowMainPage()
        {
            var articles = await _dp.Articles
                 .Where(a => a.Language == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                 .OrderByDescending(a => a.Id)
                 .Take(3)
                 .ToListAsync();

            return View("MainPage",articles);
        }
    }
}
