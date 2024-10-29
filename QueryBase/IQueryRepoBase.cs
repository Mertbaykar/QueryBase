using QueryBase.Filter;
using System.Linq.Expressions;

namespace QueryBase
{
    public interface IQueryRepoBase
    {
        /// <summary>
        /// Creates related instance for entity of type <typeparamref name="TEntity"/>. Then matches properties of <typeparamref name="TRequest"/> by name and sets values. 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="save"></param>
        /// <remarks>If all needed is to update properties with basic types (string, int, bool etc.). You could just leave <paramref name="save"/> as true. If there is more action needed with the entity, ensure <paramref name="save"/> is false. Then you could retrieve entity with basic properties updated and then proceed.</remarks>
        /// <returns></returns>
        Task<TEntity> Create<TEntity, TRequest>(TRequest request, bool save = true) where TEntity : class where TRequest : class;
        Task<TDto?> ReadFirst<TEntity, TDto>(Expression<Func<TEntity, bool>>? predicate = null) where TEntity : class where TDto : class;
        Task<TDto?> ReadById<TEntity, TDto, TKey>(TKey id) where TEntity : class, IEntityKey<TKey> where TDto : class;
        Task<TDto?> ReadSingle<TEntity, TDto>(Expression<Func<TEntity, bool>> predicate) where TEntity : class where TDto : class;
        Task<IEnumerable<TDto>> Read<TEntity, TDto>(Expression<Func<TEntity, bool>>? predicate = null, IEnumerable<QueryOrder>? queryOrders = null) where TEntity : class where TDto : class;
        /// <summary>
        /// Pagination version of ReadFiltered. Search filtering, DTO filtering and ordering happens dynamically. Matching properties by  names (allows nullable, ignores case for string properties)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="paginationFilter"></param>
        /// <param name="or">Useful for adding custom collection as an "or" expression</param>
        /// <param name="and">Useful for adding custom collection as an "and" expression</param>
        /// <returns></returns>
        Task<IEnumerable<TDto>> ReadFilteredPaginate<TEntity, TDto, TFilter>(PaginationQueryFilter<TFilter> paginationFilter, Expression<Func<TDto, bool>>? or = null, Expression<Func<TDto, bool>>? and = null) where TEntity : class where TDto : class where TFilter : class;
        /// <summary>
        /// Search filtering, DTO filtering and ordering happens dynamically. Matching properties by  names (allows nullable, ignores case for string properties)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="queryFilter"></param>
        /// <param name="or">Useful for adding custom collection as an "or" expression</param>
        /// <param name="and">Useful for adding custom collection as an "and" expression</param>
        /// <returns></returns>
        Task<IEnumerable<TDto>> ReadFiltered<TEntity, TDto, TFilter>(QueryFilter<TFilter> queryFilter, Expression<Func<TDto, bool>>? or = null, Expression<Func<TDto, bool>>? and = null) where TEntity : class where TDto : class where TFilter : class;

        /// <summary>
        /// Finds related entity of type <typeparamref name="TEntity"/> by id. Then matches properties of <typeparamref name="TRequest"/> by name and sets values.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="request"></param>
        /// <param name="save"></param>
        /// <param name="includes"></param>
        /// <remarks>If all needed is to update properties with basic types (string, int, bool etc.). You could just leave <paramref name="save"/> as true. If there is more action needed with the entity, ensure <paramref name="save"/> is false. Then you could retrieve entity with basic properties updated and then proceed. Also <paramref name="includes"/> could be used if operations needed with related entities after the basic property updates.</remarks>
        /// <returns></returns>
        Task<TEntity> Update<TEntity, TRequest, TKey>(TRequest request, bool save = true, Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>>? includes = null) where TEntity : class, IEntityKey<TKey> where TRequest : class, IEntityKey<TKey>;

        /// <summary>
        /// Finds related entity of type <typeparamref name="TEntity"/> by id. Then matches properties of <typeparamref name="TRequest"/> by name and sets values.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <param name="save"></param>
        /// <param name="includes"></param>
        /// <remarks>If all needed is to update properties with basic types (string, int, bool etc.). You could just leave <paramref name="save"/> as true. If there is more action needed with the entity, ensure <paramref name="save"/> is false. Then you could retrieve entity with basic properties updated and then proceed. Also <paramref name="includes"/> could be used if operations needed with related entities after the basic property updates.</remarks>
        /// <returns></returns>
        Task<TEntity> Update<TEntity, TRequest, TKey>(TRequest request, TKey id, bool save = true, Expression<Func<IQueryable<TEntity>, IQueryable<TEntity>>>? includes = null) where TEntity : class, IEntityKey<TKey> where TRequest : class;
        public Task BulkUpdate<TEntity>(Expression<Func<TEntity, bool>> predicate, Action<QueryBulkUpdater<TEntity>> bulkUpdateAction) where TEntity : class;
        Task Delete<TEntity>(TEntity entity, bool save = true) where TEntity : class;
        Task SoftDelete<TEntity>(TEntity entity, bool save) where TEntity : class, IEntityStatus;
        Task SoftDelete<TEntity, TKey>(TKey id, bool save = true) where TEntity : class, IEntity<TKey>;
        /// <summary>
        /// This method has a potential to create inconsistencies at database since it runs out of context transaction. It would be a solid way to wrap all the operations related to this method call in a transaction. For instance check this out: using(var transaction = context.Database.BeginTransaction()){
        /// ... OPERATIONS 
        /// ...
        /// }
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <remarks>
        /// For more info 
        /// <see href="https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete">check here</see>
        /// </remarks>
        /// <returns></returns>
        Task BulkDelete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        /// <summary>
        /// This method has a potential to create inconsistencies at database since it runs out of context transaction. It would be a solid way to wrap all the operations related to this method call in a transaction. For instance check this out: using(var transaction = context.Database.BeginTransaction()){
        /// ... OPERATIONS 
        /// ...
        /// }
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <remarks>
        /// For more info 
        /// <see href="https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete">check here</see>
        /// </remarks>
        /// <returns></returns>
        Task BulkSoftDelete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, IEntityStatus;
    }
}
