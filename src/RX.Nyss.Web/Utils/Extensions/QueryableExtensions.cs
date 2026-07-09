using System;
using System.Linq;
using System.Linq.Expressions;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageNumber, int pageSize) =>
            query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> query, Expression<Func<TSource, TKey>> keySelector, bool sortAscending) =>
            sortAscending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
    }
}
