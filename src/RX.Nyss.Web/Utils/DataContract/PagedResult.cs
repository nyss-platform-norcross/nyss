using System.Collections;

namespace RX.Nyss.Web.Utils.DataContract
{
    public class PagedResult<T>: Result<PagedResultValue<T>> where T : IEnumerable
    {
        internal PagedResult(PagedResultValue<T> value, bool isSuccess, string messageKey = null, object messageData = null)
            : base(value, isSuccess, messageKey, messageData)
        {
        }
    }

    public static  class PagedResult
    {
        public static PagedResult<T> Success<T>(T value, int page, int totalRows, int rowsPerPage, string messageKey = null, object messageData = null) where T : IEnumerable =>new PagedResult<T>(new PagedResultValue<T>(value, page, totalRows, rowsPerPage), true, messageKey, messageData);

        public static PagedResult<T> Error<T>(string messageKey, object messageData = null) where T : IEnumerable =>
            new PagedResult<T>(default, false, messageKey, messageData);
    }

    public class PagedResultValue<T>
    {
        public T Data { get; }
        public int TotalRows { get; }
        public int Page { get; }
        public int RowsPerPage { get; }

        public PagedResultValue(T data, int page, int totalRows, int rowsPerPage)
        {
            Data = data;
            TotalRows = totalRows;
            RowsPerPage = rowsPerPage;
            Page = page;
        }
    }
}
