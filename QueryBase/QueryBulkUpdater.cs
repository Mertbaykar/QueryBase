using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryBase
{
    public class QueryBulkUpdater<TEntity> where TEntity : class
    {

        internal Dictionary<LambdaExpression, Expression> PropertyUpdateExpressions = new();

        internal QueryBulkUpdater()
        {

        }

        public QueryBulkUpdater<TEntity> Update<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<TEntity, TProperty>> valueExpression)
        {
            EnsurePropertyHasJustOneUpdateCall(propertyExpression);
            PropertyUpdateExpressions.Add(propertyExpression, valueExpression);
            return this;
        }

        public QueryBulkUpdater<TEntity> Update<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            EnsurePropertyHasJustOneUpdateCall(propertyExpression);
            var valueExpression = Expression.Constant(value, typeof(TProperty));
            PropertyUpdateExpressions.Add(propertyExpression, valueExpression);
            return this;
        }

        private void EnsurePropertyHasJustOneUpdateCall<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            if (PropertyUpdateExpressions.ContainsKey(propertyExpression))
                throw new InvalidOperationException("A property cannot be updated more than once in BulkUpdate operation");
        }

        internal Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> GetBulkUpdateExpression()
        {

            ParameterExpression parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "calls");
            Expression body = parameter;

            var setPropmethods = typeof(SetPropertyCalls<TEntity>).GetMethods().Where(x => x.Name == "SetProperty");

            MethodInfo constantMethod = setPropmethods.First(x => !x.GetParameters().All(parameter => parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)));
            MethodInfo lambdaMethod = setPropmethods.Except([constantMethod]).First();

            foreach (var propertyUpdateExpression in PropertyUpdateExpressions)
            {
                Type propertyType = propertyUpdateExpression.Key.ReturnType;
                MethodInfo methodInfo;

                if (propertyUpdateExpression.Value is ConstantExpression constantExpression)
                {
                    methodInfo = constantMethod.MakeGenericMethod(propertyType);
                    body = Expression.Call(body, methodInfo, propertyUpdateExpression.Key, constantExpression);
                }
                else if (propertyUpdateExpression.Value is LambdaExpression lambdaExpression)
                {
                    methodInfo = lambdaMethod.MakeGenericMethod(propertyType);
                    body = Expression.Call(body, methodInfo, propertyUpdateExpression.Key, lambdaExpression);
                }
                else
                    throw new NotSupportedException($"Something went wrong in bulkupdate operation for type {nameof(TEntity)}");
            }

            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, parameter);
        }
    }
}
