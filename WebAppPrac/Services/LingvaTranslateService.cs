using System.Text;
using System.Text.Json;

namespace WebAppPrac.Services
{
    public class LingvaTranslateService : ItranslationService
    {
        private readonly HttpClient _httpClient;

        public LingvaTranslateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Обычный одиночный перевод
        public async Task<string> TranslateAsync(string text, string sourceLang = "ru", string targetLang = "en")
        {
            string encodedText = Uri.EscapeDataString(text);
            string url = $"/api/v1/{sourceLang}/{targetLang}/{encodedText}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json);

            return result.RootElement.GetProperty("translation").GetString() ?? string.Empty;
        }

        // Пакетный перевод (несколько строк за один запрос)
        public async Task<string[]> TranslateBatchAsync(string[] texts, string sourceLang = "ru", string targetLang = "en")
        {
            // объединяем строки через уникальный разделитель
            string combined = string.Join("|||", texts);
            string encodedText = Uri.EscapeDataString(combined);
            string url = $"/api/v1/{sourceLang}/{targetLang}/{encodedText}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(json);

            string translatedCombined = result.RootElement.GetProperty("translation").GetString() ?? string.Empty;

            // разделяем обратно
            return translatedCombined.Split("|||");
        }
    }
}
