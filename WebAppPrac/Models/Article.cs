namespace WebAppPrac.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Headline { get; set; }
        public byte[] ImageData { get; set; } //путь к изображению
        public string Subtitle { get; set; }
        public string Text { get; set; }
        public string Language { get; set; }

        public int TranslationGroupId { get; set; }

    }
}
