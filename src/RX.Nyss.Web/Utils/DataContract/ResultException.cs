using System;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Utils.DataContract
{
    public class ResultException : Exception
    {
        public Result Result { get; set; }

        private ResultException()
        {
        }

        public ResultException(string messageKey, object messageData = null)
        {
            Result = Error(messageKey, messageData);
        }

        public Result<T> GetResult<T>() => Error<T>(Result.Message.Key, Result.Message.Data);

        public Result GetResult() => Error(Result.Message.Key, Result.Message.Data);

        public override string ToString() => $"{base.ToString()}, {nameof(Result)}: {Result}";
    }
}
