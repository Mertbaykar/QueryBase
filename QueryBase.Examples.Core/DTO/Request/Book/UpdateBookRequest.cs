
namespace QueryBase.Examples.Core
{
    public class UpdateBookRequest : IEntityKey<int>
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        /// <summary>
        /// Raf yeri bilgisi
        /// </summary>
        public string ShelfLocation { get; set; }
        public int PublishYear { get; set; }
        //public IFormFile? CoverImageFile { get; set; }
        public int AuthorId { get; set; }
    }
}
