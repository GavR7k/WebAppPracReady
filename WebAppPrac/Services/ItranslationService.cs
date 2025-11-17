namespace WebAppPrac.Services
{
    public interface ItranslationService
    {
        Task<string> TranslateAsync(string text, string sourceLang = "ru", string targetLang = "en");
    }
}
