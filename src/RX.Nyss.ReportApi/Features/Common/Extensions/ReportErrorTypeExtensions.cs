using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.ReportApi.Features.Common.Extensions
{
    public static class ReportErrorTypeExtensions
    {
        public static string ToSmsErrorKey(this ReportErrorType errorType) =>
            errorType switch
            {
                ReportErrorType.DataCollectorUsedCollectionPointFormat => SmsContentKey.ReportError.FormatError,
                ReportErrorType.CollectionPointUsedDataCollectorFormat => SmsContentKey.ReportError.FormatError,
                ReportErrorType.SingleReportNonHumanHealthRisk
                    or ReportErrorType.AggregateReportNonHumanHealthRisk
                    or ReportErrorType.CollectionPointNonHumanHealthRisk
                    or ReportErrorType.EventReportHumanHealthRisk => SmsContentKey.ReportError.FormatError,
                ReportErrorType.FormatError => SmsContentKey.ReportError.FormatError,
                ReportErrorType.HealthRiskNotFound => SmsContentKey.ReportError.HealthRiskNotFound,
                ReportErrorType.Gateway => null,
                ReportErrorType.TooLong => null,
                _ => SmsContentKey.ReportError.Other
            };
    }
}
