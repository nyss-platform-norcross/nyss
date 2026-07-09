namespace RX.Nyss.Data.Concepts;

public enum ReportErrorType
{
    FormatError,
    HealthRiskNotFound,
    TooLong,
    GlobalHealthRiskCodeNotFound,
    DataCollectorUsedCollectionPointFormat,
    CollectionPointUsedDataCollectorFormat,
    CollectionPointNonHumanHealthRisk,
    SingleReportNonHumanHealthRisk,
    AggregateReportNonHumanHealthRisk,
    EventReportHumanHealthRisk,
    GenderAndAgeNonHumanHealthRisk,
    Gateway,
    Timestamp,
    Other
}
