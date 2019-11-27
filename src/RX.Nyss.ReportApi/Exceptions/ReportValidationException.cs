using System;

namespace RX.Nyss.ReportApi.Exceptions
{
    public class ReportValidationException : Exception
    {
        public ReportValidationException(string message) : base(message)
        {
        }
    }
}
