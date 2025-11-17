using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace WebAppPrac.Controllers
{
    public class CultureController : Controller
    {

        [HttpGet]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            //Дефолтное значение установка

            try
            {
                if (string.IsNullOrEmpty(culture))
                {
                    culture = "ru";
                }

                //Сохранение культуры в куки
                Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true // важно для GDPR
                    });

                if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToAction("ShowMainPage", "MainPage");

                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Прозошла ошибка");
            }

             return Redirect(returnUrl);
            
        }
    }
}
