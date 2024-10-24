

namespace QueryBase.Filter
{
    public class QueryPaginationFilter<TFilter> : QueryFilter<TFilter> where TFilter : class
    {
        public QueryPagination Pagination { get; set; }
    }
}
