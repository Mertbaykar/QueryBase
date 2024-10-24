

namespace QueryBase.Filter
{
    public class QueryPaginationFilter<TFilter> : QueryFilter<TFilter> where TFilter : class
    {
        public required QueryPagination Pagination { get; set; }
    }
}
