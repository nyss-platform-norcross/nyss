using System;

namespace RX.Nyss.Web.Features.DataContract
{
    public class ResultException : Exception
    {
        private ResultException()
        {
        }

        public ResultException(string messageKey, object messageData = null)
        {
            Result = Result.Error(messageKey, messageData);
        }

        public Result Result { get; set; }

        public override string ToString() => $"{base.ToString()}, {nameof(Result)}: {Result}";
    }
}
