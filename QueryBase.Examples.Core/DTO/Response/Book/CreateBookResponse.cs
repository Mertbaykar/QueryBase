
namespace QueryBase.Examples.Core
{
    public class CreateBookResponse
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public int PublishYear { get; set; }
        /// <summary>
        /// Server Path
        /// </summary>
        public string CoverImagePath { get; set; }
        /// <summary>
        /// Raf yeri bilgisi
        /// </summary>
        public string ShelfLocation { get; set; }
        public int AuthorId { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
