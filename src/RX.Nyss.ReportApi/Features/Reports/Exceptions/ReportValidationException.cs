using System;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports.Contracts;

namespace RX.Nyss.ReportApi.Features.Reports.Exceptions
{
    public class ReportValidationException : Exception
    {
        public ReportErrorType ErrorType { get; set; }
        public ReportValidationException(string message, ReportErrorType errorType = ReportErrorType.Other) : base(message)
        {
            ErrorType = errorType;
        }
    }
}
