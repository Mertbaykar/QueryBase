

using QueryBase.Filter;

namespace QueryBase.Examples.Core
{
    public class PaginateBookFilter : IQuerySearch
    {
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public int? PublishYear { get; set; }
        public string? ShelfLocation { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? AuthorName { get; set; }
        public string? CreatedByName { get; set; }
        public string? Search { get; set; }
    }
}
