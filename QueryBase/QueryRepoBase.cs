using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using QueryBase.Filter;
using QueryMapper;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryBase
{
    public abstract class QueryRepoBase<TContext> : IQueryRepoBase where TContext : DbContext
    {

        protected readonly TContext context;
        protected readonly IQueryMapper queryMapper;

        protected QueryRepoBase(TContext context, IQueryMapper queryMapper)
        {
            this.context = context;
            this.queryMapper = queryMapper;
        }

        public virtual async Task<TEntity> Create<TEntity, TRequest>(TRequest request, bool save = true)
            where TRequest : class
            where TEntity : class
        {
            var entity = request.Map<TEntity>(queryMapper);
            if (save)
            {
                context.Set<TEntity>().Add(entity);
                await Save();
            }
            return entity;
        }

        public virtual async Task<TDto?> ReadFirst<TEntity, TDto>(Expression<Func<TEntity, bool>>? predicate = null)
          where TDto : class
          where TEntity : class
        {
            return await BuildQuery<TEntity, TDto>(predicate).FirstOrDefaultAsync();
        }

        public virtual async Task<TDto?> ReadById<TEntity, TDto, TKey>(TKey id) where TEntity : class, IQueryKey<TKey> where TDto : class
        {
            return await ReadSingle<TEntity, TDto>(x => x.Id!.Equals(id));
        }

        public virtual async Task<TDto?> ReadSingle<TEntity, TDto>(Expression<Func<TEntity, bool>> predicate)
            where TDto : class
            where TEntity : class
        {
            return await BuildQuery<TEntity, TDto>(predicate).SingleOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TDto>> Read<TEntity, TDto>(Expression<Func<TEntity, bool>>? predicate = null, IEnumerable<QueryOrder>? queryOrders = null)
            where TDto : class
            where TEntity : class
        {
            return await BuildQuery<TEntity, TDto>(predicate, queryOrders).ToListAsync();
        }

        public virtual async Task<IEnumerable<TDto>> ReadFilteredPaginate<TEntity, TDto, TFilter>(QueryPaginationFilter<TFilter> paginationFilter, Expression<Func<TDto, bool>>? or = null, Expression<Func<TDto, bool>>? and = null) where TEntity : class where TDto : class where TFilter : class
        {
            return await BuildFilteredQuery<TEntity, TDto, TFilter>(paginationFilter, or, and)
                .Skip((paginationFilter.Pagination.Page - 1) * paginationFilter.Pagination.Size)
                .Take(paginationFilter.Pagination.Size)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<TDto>> ReadFiltered<TEntity, TDto, TFilter>(QueryFilter<TFilter> queryFilter, Expression<Func<TDto, bool>>? or, Expression<Func<TDto, bool>>? and) where TEntity : class where TDto : class where TFilter : class
        {
            return await BuildFilteredQuery<TEntity, TDto, TFilter>(queryFilter, or, and).ToListAsync();
        }

        protected IQueryable<TDto> BuildFilteredQuery<TEntity, TDto, TFilter>(QueryFilter<TFilter> queryFilter, Expression<Func<TDto, bool>>? or, Expression<Func<TDto, bool>>? and) where TEntity : class where TDto : class where TFilter : class
        {
            IQueryable<TDto> query = BuildQuery<TEntity, TDto>();
            var filterExpression = CreateFilterExpression(queryFilter.Filter, or, and);
            if (filterExpression != null)
                query = query.Where(filterExpression);
            return BuildOrder(query, queryFilter.Order);
        }

        /// <summary>
        /// Creates filter expression for properties of type <typeparamref name="TDto"/> by matching properties of <typeparamref name="TFilter"/> where filter property values are not null and search value is not empty
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="filter"></param>
        /// <param name="or"></param>
        /// <param name="and"></param>
        /// <returns></returns>
        /// <remarks>Properties are matching based on their names. Ignores case for string filtering. Could be used in various scenarios beside its current usage. For example when you need to retrieve data from a different data provider and have a need for expression</remarks>
        /// <exception cref="InvalidOperationException"></exception>
        protected Expression<Func<TDto, bool>>? CreateFilterExpression<TDto, TFilter>(TFilter filter, Expression<Func<TDto, bool>>? or = null, Expression<Func<TDto, bool>>? and = null) where TDto : class where TFilter : class
        {
            var parameter = Expression.Parameter(typeof(TDto), typeof(TDto).Name);
            Expression? combinedExpression = null;
            Expression comparison;
            bool isTypeString;

            #region Search filter

            if (filter is IQuerySearch filterSearchable && !string.IsNullOrWhiteSpace(filterSearchable.Search))
            {
                foreach (var dtoProp in typeof(TDto).GetProperties().Where(x => x.CanRead))
                {
                    // IEnumerable, class and datetime eliminated
                    if ((dtoProp.PropertyType.IsPrimitive || dtoProp.PropertyType.IsEnum || IsSameType(dtoProp.PropertyType, typeof(string)))
                        && !IsSameType(dtoProp.PropertyType, typeof(DateTime)))
                    {
                        var dtoPropExpression = Expression.Property(parameter, dtoProp.Name);
                        var searchValueExp = Expression.Constant(filterSearchable.Search.ToLower());

                        Expression hasValue;
                        Expression valueExp;

                        // Nullable type ise
                        if (Nullable.GetUnderlyingType(dtoProp.PropertyType) is Type dtoRealType)
                        {
                            isTypeString = dtoRealType == typeof(string);

                            if (isTypeString)
                            {
                                valueExp = dtoPropExpression;
                                valueExp = Expression.Call(valueExp, QueryableMethods.toLowerMethod);
                                valueExp = Expression.Call(valueExp, QueryableMethods.containsMethod, searchValueExp);
                                comparison = Expression.Equal(valueExp, searchValueExp);
                            }
                            else
                            {
                                hasValue = Expression.Property(dtoPropExpression, "HasValue");
                                valueExp = Expression.Property(dtoPropExpression, "Value");
                                valueExp = Expression.Call(valueExp, QueryableMethods.toStringMethod);
                                valueExp = Expression.Call(valueExp, QueryableMethods.toLowerMethod);
                                valueExp = Expression.Call(valueExp, QueryableMethods.containsMethod, searchValueExp);
                                comparison = Expression.AndAlso(
                                    hasValue,
                                    valueExp);
                            }

                        }
                        // not nullable
                        else
                        {
                            valueExp = dtoPropExpression;

                            if (dtoProp.PropertyType != typeof(string))
                                valueExp = Expression.Call(dtoPropExpression, QueryableMethods.toStringMethod);

                            valueExp = Expression.Call(valueExp, QueryableMethods.toLowerMethod);
                            comparison = Expression.Call(valueExp, QueryableMethods.containsMethod, searchValueExp);
                        }

                        if (combinedExpression == null)
                            combinedExpression = comparison;
                        else
                            combinedExpression = Expression.OrElse(combinedExpression, comparison);
                    }
                }
            }

            #endregion

            #region Filter DTO query using Filter Model, via matching property names and types (allows nullable, ignores case for string properties)

            // prop filtering via name matching
            foreach (var filterProp in typeof(TFilter).GetProperties().Where(x => x.CanRead))
            {
                if (filterProp.Name == nameof(IQuerySearch.Search))
                    continue;
                if (IsSameType(filterProp.PropertyType, typeof(DateTime)))
                    continue;
                // IEnumerable, class and datetime eliminated
                if (!((filterProp.PropertyType.IsPrimitive || filterProp.PropertyType.IsEnum || IsSameType(filterProp.PropertyType, typeof(string)))
                    && !IsSameType(filterProp.PropertyType, typeof(DateTime))))
                    continue;

                var filterValue = filterProp.GetValue(filter);
                if (filterValue == null || string.IsNullOrEmpty(filterValue.ToString()))
                    continue;

                var dtoProp = typeof(TDto).GetProperty(filterProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                // types should be equal, could be nullable too
                if (dtoProp == null || !dtoProp.CanRead)
                    continue;
                if (!IsSameType(dtoProp.PropertyType, filterProp.PropertyType))
                    throw new InvalidOperationException($"{dtoProp.Name} property has {dtoProp.PropertyType.Name} type. {filterProp.Name} property has {filterProp.PropertyType.Name} type. They should match for filtering");

                var dtoPropExpression = Expression.Property(parameter, dtoProp.Name);
                bool isDtoPropString = IsSameType(dtoProp.PropertyType, typeof(string));
                if (isDtoPropString)
                    filterValue = filterValue.ToString()!.ToLower();
                var searchValueExp = Expression.Constant(filterValue);

                Expression valueExp = dtoPropExpression;

                // nullable
                if (Nullable.GetUnderlyingType(dtoProp.PropertyType) is Type dtoNullableType)
                {
                    if (isDtoPropString)
                    {
                        // string ise tolower gelmeli, nullable olduğunda != null gelmeli
                        Expression hasValue = Expression.NotEqual(dtoPropExpression, Expression.Constant(null, dtoNullableType));
                        valueExp = Expression.Call(valueExp, QueryableMethods.toLowerMethod);
                        valueExp = Expression.Call(valueExp, QueryableMethods.containsMethod, searchValueExp);
                        comparison = Expression.AndAlso(hasValue, valueExp);
                    }
                    else
                        comparison = Expression.Equal(valueExp, searchValueExp);
                }
                // not nullable
                else
                {
                    if (isDtoPropString)
                    {
                        valueExp = Expression.Call(valueExp, QueryableMethods.toLowerMethod);
                        valueExp = Expression.Call(valueExp, QueryableMethods.containsMethod, searchValueExp);
                        comparison = valueExp;
                    }
                    else
                        comparison = Expression.Equal(valueExp, searchValueExp);
                }

                if (combinedExpression == null)
                    combinedExpression = comparison;
                else
                    combinedExpression = Expression.AndAlso(combinedExpression, comparison);
            }

            #endregion

            if (or != null)
            {
                var replacer = new ParameterReplacer(or.Parameters[0], parameter);
                var updatedOr = replacer.Visit(or.Body);
                combinedExpression = combinedExpression == null ? updatedOr : Expression.OrElse(combinedExpression, updatedOr);
            }

            if (and != null)
            {
                var replacer = new ParameterReplacer(and.Parameters[0], parameter);
                var updatedAnd = replacer.Visit(and.Body);
                combinedExpression = combinedExpression == null ? updatedAnd : Expression.AndAlso(combinedExpression, updatedAnd);
            }

            return combinedExpression != null
                ? Expression.Lambda<Func<TDto, bool>>(combinedExpression, parameter)
                : null;
        }

        protected IQueryable<TDto> BuildQuery<TEntity, TDto>(Expression<Func<TEntity, bool>>? predicate = null, IEnumerable<QueryOrder>? queryOrders = null)
            where TDto : class
            where TEntity : class
        {
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (predicate != null)
                query = query.Where(predicate);
            IQueryable<TDto> result = query.Map<TDto>(queryMapper);
            return BuildOrder(result, queryOrders);
        }

        /// <summary>
        /// Builds ordering of <paramref name="query"/>
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="query"></param>
        /// <param name="queryOrders"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IQueryable<TDto> BuildOrder<TDto>(IQueryable<TDto> query, IEnumerable<QueryOrder>? queryOrders = null)
          where TDto : class
        {

            if (queryOrders != null && queryOrders.Any())
            {
                if (queryOrders.Any(x => string.IsNullOrWhiteSpace(x.PropertyName)))
                    throw new Exception($"There shouldn't be any empty propertyname while ordering. Error occurred while ordering {typeof(TDto).Name}");

                bool isFirstOrder = true;

                foreach (QueryOrder order in queryOrders)
                {
                    PropertyInfo? prop = typeof(TDto).GetProperty(order.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (prop == null)
                        throw new Exception($"Property names should match while ordering. Error occurred while ordering {typeof(TDto).Name}");

                    var dtoParam = Expression.Parameter(typeof(TDto), typeof(TDto).Name);
                    var propExp = Expression.Property(dtoParam, order.PropertyName);
                    // keySelector
                    var lambda = Expression.Lambda(propExp, dtoParam);

                    MethodInfo method = isFirstOrder ? (order.Asc ? QueryableMethods.OrderByMethod : QueryableMethods.OrderByDescendingMethod) : (order.Asc ? QueryableMethods.ThenByMethod : QueryableMethods.ThenByDescendingMethod);

                    method = method.MakeGenericMethod(typeof(TDto), prop.PropertyType);
                    query = (IQueryable<TDto>)method.Invoke(null, [query, lambda])!;
                    isFirstOrder = false;
                }
            }

            return query;
        }

        /// <summary>
        /// Builds ordering of <paramref name="collection"/>
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="collection"></param>
        /// <param name="queryOrders"></param>
        /// <remarks>Could be useful for different data providers and other custom scenarios</remarks>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IEnumerable<TDto> BuildOrder<TDto>(IEnumerable<TDto> collection, IEnumerable<QueryOrder>? queryOrders = null)
        where TDto : class
        {

            if (queryOrders != null && queryOrders.Any())
            {
                if (queryOrders.Any(x => string.IsNullOrWhiteSpace(x.PropertyName)))
                    throw new Exception($"There shouldn't be any empty propertyname while ordering. Error occurred while ordering {typeof(TDto).Name}");

                bool isFirstOrder = true;

                foreach (QueryOrder order in queryOrders)
                {
                    PropertyInfo? prop = typeof(TDto).GetProperty(order.PropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (prop == null)
                        throw new Exception($"Property names should match while ordering. Error occurred while ordering {typeof(TDto).Name}");

                    var dtoParam = Expression.Parameter(typeof(TDto), typeof(TDto).Name);
                    var propExp = Expression.Property(dtoParam, order.PropertyName);
                    // keySelector
                    var lambda = Expression.Lambda(propExp, dtoParam);

                    MethodInfo method = isFirstOrder ? (order.Asc ? QueryableMethods.OrderByMethod : QueryableMethods.OrderByDescendingMethod) : (order.Asc ? QueryableMethods.ThenByMethod : QueryableMethods.ThenByDescendingMethod);

                    method = method.MakeGenericMethod(typeof(TDto), prop.PropertyType);
                    collection = (IEnumerable<TDto>)method.Invoke(null, [collection, lambda.Compile()])!;
                    isFirstOrder = false;
                }
            }

            return collection;
        }

        public virtual async Task<TEntity> Update<TEntity, TRequest, TKey>(TRequest request, bool save = true, Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>>? includes = null) where TEntity : class, IQueryKey<TKey> where TRequest : class, IQueryKey<TKey>
        {
            IQueryable<TEntity> query = context.Set<TEntity>().Where(x => x.Id!.Equals(request.Id));

            // Apply includes if provided
            if (includes != null)
                query = ApplyIncludes(query, includes);

            var existingEntity = await query.SingleOrDefaultAsync();
            if (existingEntity == null)
                throw new InvalidOperationException($"{typeof(TEntity)} entity with {request.Id} id not found.");

            context.Entry(existingEntity).CurrentValues.SetValues(request);

            if (save)
                await Save();
            return existingEntity;
        }

        private IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> query, Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includes) where TEntity : class
        {
            if (!HasOnlyIncludeRelatedMethods(includes.Body))
                throw new ArgumentException($"{nameof(includes)} must contain only include related methods", nameof(includes));
            return includes.Compile()(query);
        }

        private bool HasOnlyIncludeRelatedMethods(Expression expression)
        {
            if (expression is MethodCallExpression methodExpression)
            {
                if (QueryableMethods.IncludeMethods.Contains(methodExpression.Method.Name))
                {
                    var callingExp = methodExpression.Arguments.FirstOrDefault();

                    if (callingExp != null)
                        return HasOnlyIncludeRelatedMethods(callingExp);
                }

                return false;
            }

            if (expression is ParameterExpression)
                return true;

            return false;
        }

        public virtual async Task Delete<TEntity>(TEntity entity, bool save = true) where TEntity : class
        {
            context.Entry(entity).State = EntityState.Deleted;
            if (save)
                await Save();
        }

        public virtual async Task SoftDelete<TEntity>(TEntity entity, bool save = true) where TEntity : class, IQueryStatus
        {
            entity.DeActivate();
            if (save)
                await Save();
        }

        public virtual async Task SoftDelete<TEntity, TKey>(TKey id, bool save = true) where TEntity : class, IQueryEntity<TKey>
        {
            var entity = context.Set<TEntity>().Find(id);
            if (entity == null)
                throw new InvalidOperationException($"{typeof(TEntity).Name} not found with key {id}");
            await SoftDelete(entity, save);
        }

        public virtual async Task BulkDelete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            await context.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();
        }

        public virtual async Task BulkSoftDelete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, IQueryStatus
        {
            await BulkUpdate(predicate, x => x.Update(y => y.IsActive, false));
        }

        public virtual async Task BulkUpdate<TEntity>(Expression<Func<TEntity, bool>> predicate, Action<QueryBulkUpdater<TEntity>> bulkUpdateAction) where TEntity : class
        {
            var bulkUpdater = new QueryBulkUpdater<TEntity>();
            bulkUpdateAction.Invoke(bulkUpdater);
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExp = bulkUpdater.GetBulkUpdateExpression();
            await context.Set<TEntity>().Where(predicate).ExecuteUpdateAsync(updateExp);
        }

        protected virtual async Task<int> Save()
        {
            return await context.SaveChangesAsync();
        }

        /// <summary>
        /// Matches types including their nullability
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        private bool IsSameType(Type type1, Type type2)
        {
            return type1 == type2 || Nullable.GetUnderlyingType(type1) == type2 || Nullable.GetUnderlyingType(type2) == type1;
        }

    }

    internal static class QueryableMethods
    {

        #region String

        internal static readonly MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        internal static readonly MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        internal static readonly MethodInfo toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes)!;

        #endregion

        #region Order

        internal static readonly MethodInfo OrderByMethod = GetOrderingMethod("OrderBy");
        internal static readonly MethodInfo OrderByDescendingMethod = GetOrderingMethod("OrderByDescending");
        internal static readonly MethodInfo ThenByMethod = GetOrderingMethod("ThenBy");
        internal static readonly MethodInfo ThenByDescendingMethod = GetOrderingMethod("ThenByDescending");

        #endregion

        #region Include

        internal static readonly List<string> IncludeMethods = ["Include", "ThenInclude", "AsSplitQuery", "AsSingleQuery"];

        #endregion

        private static MethodInfo GetOrderingMethod(string methodName)
        {
            return typeof(Queryable).GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single();
        }
    }
}
