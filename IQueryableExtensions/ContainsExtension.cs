using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IQueryableExtensions
{
    public static class ContainsExtension
    {
        public static IQueryable<TQuery> In<TKey, TQuery>(
            this IQueryable<TQuery> queryable,
            IEnumerable<TKey> values,
            Expression<Func<TQuery, TKey>> keySelector)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (!values.Any())
            {
                return queryable.Take(0);
            }

            values = values.Distinct();

            if (values.Count() > 2048)
            {
                throw new ArgumentException("Too many parameters for SQL Server, reduce the number of parameters", nameof(keySelector));
            }

            var predicates = values
                .Select(v =>
                {
                    // Create an expression that captures the variable so EF can turn this into a parameterized SQL query
                    Expression<Func<TKey>> valueAsExpression = () => v;
                    return Expression.Equal(keySelector.Body, valueAsExpression.Body);
                })
                .ToList();

            var result = predicates.Aggregate((x, y)=> Expression.OrElse(x, y));


            var clause = Expression.Lambda<Func<TQuery, bool>>(result, keySelector.Parameters);

            return queryable.Where(clause);
        }
    }
}
