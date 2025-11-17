
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAppPrac.Services
{
    public class LibreTranslateService : ItranslationService
    {
        private readonly HttpClient _httpClient;

        public LibreTranslateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> TranslateAsync(string text, string sourceLang = "ru", string targetLang = "en")
        {

            //тело запроса
            var requesstBody = new
            {
                q = text,
                source = sourceLang,
                target = targetLang,
                format = "text"
            };

            //контент который сериализуется
            var content = new StringContent(JsonSerializer.Serialize(requesstBody), Encoding.UTF8, "application/json");


            // Отправляем POST-запрос на API LibreTranslate с нашим контентом
            var response = await _httpClient.PostAsync("/translate", content);
            // Ждём ответа от сервера и проверяем, что статус успешный (200 OK и т.п.)
            response.EnsureSuccessStatusCode();

            // Читаем тело ответа как строку (ожидаем JSON)
            var json = await response.Content.ReadAsStringAsync();

            // Парсим JSON-строку в объект JsonDocument
            var result = JsonDocument.Parse(json);

            // Извлекаем значение поля "translatedText" это переведенный текст
            return result.RootElement.GetProperty("translatedText").GetString();

        }
    }
}
