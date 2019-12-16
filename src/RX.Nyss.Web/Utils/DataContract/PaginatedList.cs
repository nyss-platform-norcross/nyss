using System.Collections.Generic;

namespace RX.Nyss.Web.Utils.DataContract
{
    public class PaginatedList<T>
    {
        public IList<T> Data { get; }
        public int TotalRows { get; }
        public int Page { get; }
        public int RowsPerPage { get; }

        public PaginatedList(IList<T> data, int page, int totalRows, int rowsPerPage)
        {
            Data = data;
            TotalRows = totalRows;
            RowsPerPage = rowsPerPage;
            Page = page;
        }
    }
}
