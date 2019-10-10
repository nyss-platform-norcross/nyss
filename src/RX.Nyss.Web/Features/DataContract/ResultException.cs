using System;

namespace RX.Nyss.Web.Features.DataContract
{
    public class ResultException : Exception
    {
        private ResultException()
        {
        }

        public ResultException(string messageKey, object messageData)
        {
            Result = Result.Error(messageKey, messageData);
        }

        public Result Result { get; set; }

        public static void Throw(string messageKey, object messageData = null)
            => throw new ResultException(messageKey, messageData);

        public override string ToString() => $"{base.ToString()}, {nameof(Result)}: {Result}";
    }
}
