using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.RegularExpressions;
using WebAppPrac.Data;
using WebAppPrac.Models;
using WebAppPrac.Services;

namespace WebAppPrac.Controllers
{
    public class ArticleController : Controller
    {

        private readonly IStringLocalizer<ArticleController> _localizer;
        private readonly ItranslationService _translator;
        private readonly AppDbContext _db;
        public ArticleController(IStringLocalizer<ArticleController> localizer, ItranslationService translator, AppDbContext db)
        {
            _localizer = localizer;
            _translator = translator;
            _db = db;
        }

        //Проверки на правильный ввод языка для русских и английских символов

        //bool ContainsCyrillicLettersOnly(string input)
        //{
        //    return Regex.IsMatch(input, @"\p{IsCyrillic}") && !Regex.IsMatch(input, @"[A-Za-z]");
        //}

        //bool ContainsLatinLettersOnly(string input)
        //{
        //    return Regex.IsMatch(input, @"[A-Za-z]") && !Regex.IsMatch(input, @"\p{IsCyrillic}");
        //}

        [HttpGet]
        public IActionResult ShowFormForCreatingArticle()
        {
            return View("FormForArticle");
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle(Article model)
        {
            var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var form = Request.Form;
            var file = form.Files.GetFile("image");

            model.Language = currentLanguage;
            model.ImageData = await ConvertImageToBytes(file);

            await _db.Articles.AddAsync(model);
            await _db.SaveChangesAsync();

            model.TranslationGroupId = model.Id;
            _db.Articles.Update(model);
            await _db.SaveChangesAsync();

            if (currentLanguage == "ru")
            {
                // пакетный перевод трёх полей
                var translations = await _translator.TranslateBatchAsync(
                    new[] { model.Headline, model.Subtitle, model.Text },
                    "ru", "en"
                );

                Article enMode = new Article
                {
                    Headline = translations[0],
                    Subtitle = translations[1],
                    Text = translations[2],
                    Language = "en",
                    TranslationGroupId = model.Id,
                    ImageData = model.ImageData
                };

                await _db.Articles.AddAsync(enMode);
                await _db.SaveChangesAsync();
            }
            else if (currentLanguage == "en")
            {
                var translations = await _translator.TranslateBatchAsync(
                    new[] { model.Headline, model.Subtitle, model.Text },
                    "en", "ru"
                );

                Article ruMode = new Article
                {
                    Headline = translations[0],
                    Subtitle = translations[1],
                    Text = translations[2],
                    Language = "ru",
                    TranslationGroupId = model.Id,
                    ImageData = model.ImageData
                };

                await _db.Articles.AddAsync(ruMode);
                await _db.SaveChangesAsync();
            }

            TempData["ArticleSuccessMessage"] = currentLanguage == "ru"
                ? "Запись успешно добавлена"
                : "Article successfully added";

            return RedirectToAction("ShowMainPage", "MainPage");
        }


        private async Task<byte[]> ConvertImageToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }


        public async Task<IActionResult> DeleteArticle(int id)
        {
            //удалить запись из бд по id ?
            var article = await _db.Articles.FindAsync(id);

            if (article == null)
            {
                TempData["ArticleNoDeleted"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
            ? "Запись не найдена"
            : "Article not found";
                return RedirectToAction("ShowMainPage", "MainPage");

            }
            //вернуть представление с записями
            var groupId = article.TranslationGroupId;
            var groupArticles = await _db.Articles.Where(x => x.TranslationGroupId == groupId).ToListAsync();

            _db.Articles.RemoveRange(groupArticles);
            await _db.SaveChangesAsync();
            TempData["ArticleSuccessDeleted"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
            ? "Запись успешно удалена"
            : "Article successfully deleted";

            return RedirectToAction("ShowMainPage", "MainPage");

        }

        [HttpGet]
        public async Task<IActionResult> EditArticle(int id)
        {
            var original = await _db.Articles.FindAsync(id);

            if (original == null)
            {
                TempData["ArticleNoDeleted"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
                    ? "Запись не найдена"
                    : "Article not found";
                return RedirectToAction("ShowMainPage", "MainPage");
            }

            var currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var groupId = original.TranslationGroupId;

            var localizedVersion = await _db.Articles
                .FirstOrDefaultAsync(x => x.TranslationGroupId == groupId && x.Language == currentLang);

            if (localizedVersion == null)
            {
                TempData["ArticleNoDeleted"] = currentLang == "ru"
                    ? "Локализованная версия не найдена"
                    : "Localized version not found";
                return RedirectToAction("ShowMainPage", "MainPage");
            }

            return View(localizedVersion); // передача представления в форму
        }

        [HttpPost]
        public async Task<IActionResult> EditArticle(int id, Article updatedArticle, IFormFile image)
        {
            var article = await _db.Articles.FindAsync(id);

            if (article == null)
            {
                TempData["ArticleNoDeleted"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
                    ? "Запись не найдена"
                    : "Article not found";
                return RedirectToAction("ShowMainPage", "MainPage");
            }

            var groupId = article.TranslationGroupId;
            var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // обновляем текущую версию
            article.Headline = updatedArticle.Headline;
            article.Subtitle = updatedArticle.Subtitle;
            article.Text = updatedArticle.Text;

            if (image != null)
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                article.ImageData = memoryStream.ToArray();
            }

            if (currentLanguage == "ru")
            {
                // найти английскую версию
                var englishVersion = await _db.Articles
                    .FirstOrDefaultAsync(x => x.TranslationGroupId == groupId && x.Language == "en");

                // пакетный перевод
                var translations = await _translator.TranslateBatchAsync(
                    new[] { article.Headline, article.Subtitle, article.Text },
                    "ru", "en"
                );

                englishVersion.Headline = translations[0];
                englishVersion.Subtitle = translations[1];
                englishVersion.Text = translations[2];
                englishVersion.ImageData = article.ImageData;

                _db.Articles.UpdateRange(article, englishVersion);
            }
            else if (currentLanguage == "en")
            {
                // найти русскую версию
                var russianVersion = await _db.Articles
                    .FirstOrDefaultAsync(x => x.TranslationGroupId == groupId && x.Language == "ru");

                var translations = await _translator.TranslateBatchAsync(
                    new[] { article.Headline, article.Subtitle, article.Text },
                    "en", "ru"
                );

                russianVersion.Headline = translations[0];
                russianVersion.Subtitle = translations[1];
                russianVersion.Text = translations[2];
                russianVersion.ImageData = article.ImageData;

                _db.Articles.UpdateRange(article, russianVersion);
            }

            await _db.SaveChangesAsync();

            TempData["ArticleSuccessMessage"] = currentLanguage == "ru"
                ? "Запись успешно обновлена"
                : "Article successfully updated";

            return RedirectToAction("ShowMainPage", "MainPage");
        }

    }
}
