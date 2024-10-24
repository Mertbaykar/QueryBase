using System.Text.Json.Serialization;

namespace QueryBase.Examples.Core
{
    public class CreateBookRequest
    {

        public string Name { get; set; }
        public string Summary { get; set; }
        /// <summary>
        /// Raf yeri bilgisi
        /// </summary>
        public string ShelfLocation { get; set; }
        public int PublishYear { get; set; }
        /// <summary>
        /// Server Path, canlıda network paylaşımlı bir IIS dizini veya azure blob storage tarzı bir şey kullanılabilir. Gösterim amaçlı server'a kaydediliyor
        /// </summary>
        [JsonIgnore]
        public string? CoverImagePath { get; set; }
        //public IFormFile CoverImageFile { get; set; }

        public int AuthorId { get; set; }

        public int CreatedById { get; set; }
    }
}
