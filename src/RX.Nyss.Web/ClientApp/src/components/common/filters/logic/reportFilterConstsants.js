export const DataCollectorType = {
  unknownSender: "UnknownSender",
  human: "Human",
  collectionPoint: "CollectionPoint",
};

export const ReportErrorFilterType = {
  all: "All",
  healthRiskNotFound: "HealthRiskNotFound",
  wrongFormat: "WrongFormat",
  other: "Other",
};

export const reportErrorFilterTypes = [
  ReportErrorFilterType.all,
  ReportErrorFilterType.healthRiskNotFound,
  ReportErrorFilterType.wrongFormat,
  ReportErrorFilterType.other,
];

export const correctedStateTypes = ["All", "Corrected", "NotCorrected"];
