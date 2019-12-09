using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class PaginatedDataExtensions
    {
        public static PaginatedList<T> AsPaginatedList<T>(this IEnumerable<T> data, int page, int totalRows, int rowsPerPage) =>
            new PaginatedList<T>(data.ToList(), page, totalRows, rowsPerPage);

        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> data, int page, int totalRows, int rowsPerPage) =>
            new PaginatedList<T>(await data.ToListAsync(), page, totalRows, rowsPerPage);
    }
}
