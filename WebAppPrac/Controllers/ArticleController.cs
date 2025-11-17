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
            #region
            //использовние проверки на ввод симовлов языка

            //bool isValid = currentLanguage switch
            //{
            //    "ru" => ContainsCyrillicLettersOnly(headline) &&
            //    ContainsCyrillicLettersOnly(subtitle) &&
            //    ContainsCyrillicLettersOnly(text),
            //    "en" => ContainsLatinLettersOnly(headline) &&
            //    ContainsLatinLettersOnly(subtitle) &&
            //    ContainsLatinLettersOnly(text),
            //    _ => false
            //};
            //if (!isValid)
            //{
            //    ViewBag.ArticleError = _localizer["UseCorrectFormOfLanguage"];
            //    return View("FormForArticle");
            //}
            //else
            //{
            //    //создаем два объекта с новостями
            //    throw new NotImplementedException();

            //}
            #endregion

            //проверка какой текущий язык был установлен в Culture.Info
            var currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (currentLanguage == "ru")
            {
                var form = Request.Form;
                var file = form.Files.GetFile("image");

                //  model.TranslationGroupId = model.Id;
                model.Language = currentLanguage;
                model.ImageData = await ConvertImageToBytes(file);

                await _db.Articles.AddAsync(model);
                await _db.SaveChangesAsync();

                model.TranslationGroupId = model.Id;
                _db.Articles.Update(model);
                await _db.SaveChangesAsync();

                //создаем объект для англ версии
                var translatedHeadline = await _translator.TranslateAsync(model.Headline);
                var translatedSubtitle = await _translator.TranslateAsync(model.Subtitle);
                var translatedText = await _translator.TranslateAsync(model.Text);

                Article enMode = new Article
                {
                    Headline = translatedHeadline,
                    Subtitle = translatedSubtitle,
                    Text = translatedText,
                    Language = "en",
                    TranslationGroupId = model.Id,
                    ImageData = model.ImageData
                };

                await _db.Articles.AddAsync(enMode);
                await _db.SaveChangesAsync();
                //отправляем в апи запрос на перевод нужных полей
                //после получения создаем объект для англ версии
            }
            if (currentLanguage == "en")
            {
                var form = Request.Form;
                var file = form.Files.GetFile("image");

                model.Language = currentLanguage;
                model.ImageData = await ConvertImageToBytes(file);

                await _db.Articles.AddAsync(model);
                await _db.SaveChangesAsync();

                model.TranslationGroupId = model.Id;
                _db.Articles.Update(model);
                await _db.SaveChangesAsync();

                // создаем русскую версию
                var translatedHeadline = await _translator.TranslateAsync(model.Headline, "en", "ru");
                var translatedSubtitle = await _translator.TranslateAsync(model.Subtitle, "en", "ru");
                var translatedText = await _translator.TranslateAsync(model.Text, "en", "ru");

                Article ruMode = new Article
                {
                    Headline = translatedHeadline,
                    Subtitle = translatedSubtitle,
                    Text = translatedText,
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
            //Написать функционал для создания новости
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
            //сделай тут еще проверку для обоих языков
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru")
            {
                //найти из таблицы запись где TranslationgroupId = groupId и Language английский
                var englishVersion = await _db.Articles.FirstOrDefaultAsync(x => x.TranslationGroupId == groupId && x.Language == "en");

                article.Headline = updatedArticle.Headline;
                article.Subtitle = updatedArticle.Subtitle;
                article.Text = updatedArticle.Text;

                if (image != null)
                {
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    article.ImageData = memoryStream.ToArray();
                }

                englishVersion.Headline = await _translator.TranslateAsync(article.Headline);
                englishVersion.Subtitle = await _translator.TranslateAsync(article.Subtitle);
                englishVersion.Text = await _translator.TranslateAsync(article.Text);
                englishVersion.ImageData = article.ImageData;


                List<Article> list = new List<Article>()
                {
                    article,englishVersion
                };
                _db.Articles.UpdateRange(list);

            }
            if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en")
            {
                var russianVersion = await _db.Articles.FirstOrDefaultAsync(x => x.TranslationGroupId == groupId && x.Language == "ru");

                article.Headline = updatedArticle.Headline;
                article.Subtitle = updatedArticle.Subtitle;
                article.Text = updatedArticle.Text;

                if (image != null)
                {
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    article.ImageData = memoryStream.ToArray();
                }

                russianVersion.Headline = await _translator.TranslateAsync(article.Headline, "en", "ru");
                russianVersion.Subtitle = await _translator.TranslateAsync(article.Subtitle, "en", "ru");
                russianVersion.Text = await _translator.TranslateAsync(article.Text, "en", "ru");
                russianVersion.ImageData = article.ImageData;


                List<Article> list = new List<Article>()
                {
                    article,russianVersion
                };
                _db.Articles.UpdateRange(list);
            }
            await _db.SaveChangesAsync();

            TempData["ArticleSuccessMessage"] = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
                ? "Запись успешно обновлена"
                : "Article successfully updated";

            return RedirectToAction("ShowMainPage", "MainPage");
        }


    }
}
