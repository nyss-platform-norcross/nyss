namespace RX.Nyss.Data.Concepts
{
    public static class ReportErrorTypeExtensions
    {
        public static string ToSmsErrorKey(this ReportErrorType errorType) =>
            errorType switch
            {
                ReportErrorType.DataCollectorUsedCollectionPointFormat => SmsContentKey.ReportError.DataCollectorUsedCollectionPointFormat,
                ReportErrorType.CollectionPointUsedDataCollectorFormat => SmsContentKey.ReportError.CollectionPointUsedDataCollectorFormat,
                ReportErrorType.SingleReportNonHumanHealthRisk
                    or ReportErrorType.AggregateReportNonHumanHealthRisk
                    or ReportErrorType.CollectionPointNonHumanHealthRisk=> SmsContentKey.ReportError.FormatCannotBeUsedForNonHumanHealthRisk,
                ReportErrorType.HealthRiskNotFound => SmsContentKey.ReportError.HealthRiskNotFound,
                ReportErrorType.Gateway => SmsContentKey.ReportError.Gateway,
                ReportErrorType.FormatError => SmsContentKey.ReportError.FormatError,
                _ => SmsContentKey.ReportError.Other,
            };
    }
}
