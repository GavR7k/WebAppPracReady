using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebAppPrac.Data;
using X.PagedList;
using X.PagedList.Extensions;
namespace WebAppPrac.Controllers
{
    public class AllNewsPageController : Controller
    {

        private readonly AppDbContext _db;

        public AllNewsPageController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> ShowAllNews(int? pageNumber)
        {
            int pageSize = 4;
            int pageIndex = pageNumber ?? 1;
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var articles = _db.Articles
                .Where(a => a.Language == lang)
                .OrderByDescending(a => a.Id);

            var pagedArticles = articles.ToPagedList(pageIndex, pageSize);
            return View("AllNewsPage",pagedArticles);
        }
    }
}
