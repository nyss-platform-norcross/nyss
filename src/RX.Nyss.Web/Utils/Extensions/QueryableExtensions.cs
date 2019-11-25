using System.Linq;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageNumber, int pageSize) =>
            query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
