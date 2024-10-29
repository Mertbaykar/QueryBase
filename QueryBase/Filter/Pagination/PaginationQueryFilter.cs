

namespace QueryBase.Filter
{
    public class PaginationQueryFilter<TFilter> : QueryFilter<TFilter> where TFilter : class
    {
        public required Pagination Pagination { get; set; }
    }
}
