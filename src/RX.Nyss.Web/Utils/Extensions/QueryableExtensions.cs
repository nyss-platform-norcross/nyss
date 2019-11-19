using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageNumber, int pageSize = 50) =>
            query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
