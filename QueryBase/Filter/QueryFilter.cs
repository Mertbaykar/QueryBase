

namespace QueryBase.Filter
{
    public class QueryFilter<TFilter> where TFilter : class
    {
        public TFilter Filter { get; set; }
        public IEnumerable<QueryOrder>? Order { get; set; }
    }
}
