using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
