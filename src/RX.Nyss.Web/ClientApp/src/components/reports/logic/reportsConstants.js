import { action } from "../../../utils/actions";

export const OPEN_CORRECT_REPORTS_LIST = action("OPEN_CORRECT_REPORTS_LIST");
export const OPEN_INCORRECT_REPORTS_LIST = action(
  "OPEN_INCORRECT_REPORTS_LIST",
);
export const GET_CORRECT_REPORTS = action("GET_CORRECT_REPORTS");
export const GET_INCORRECT_REPORTS = action("GET_INCORRECT_REPORTS");
export const OPEN_REPORT_EDITION = action("OPEN_REPORT_EDITION");
export const EDIT_REPORT = action("EDIT_REPORT");
export const EXPORT_TO_CSV = action("EXPORT_TO_CSV");
export const EXPORT_TO_EXCEL = action("EXPORT_TO_EXCEL");
export const MARK_AS_ERROR = action("MARK_AS_ERROR");
export const OPEN_SEND_REPORT = action("OPEN_SEND_REPORT");
export const ACCEPT_REPORT = action("ACCEPT_REPORT_IN_LIST");
export const DISMISS_REPORT = action("DISMISS_REPORT_IN_LIST");
export const TRACK_REPORT_EXPORT = action("TRACK_REPORT_EXPORT");
export const MARK_AS_CORRECTED = action("MARK_AS_CORRECTED");
export const MARK_AS_NOT_CORRECTED = action("MARK_AS_NOT_CORRECTED");

export const DateColumnName = "date";

export const reportStatus = {
  new: "New",
  pending: "Pending",
  accepted: "Accepted",
  rejected: "Rejected",
  closed: "Closed",
};

export const reportSexes = {
  male: "male",
  female: "female",
  unspecified: "unspecified",
};

export const reportAges = {
  belowFive: "belowFive",
  aboveFour: "aboveFour",
  unspecified: "unspecified",
};

export const reportCountToSexAge = {
  countMalesAtLeastFive: {
    sex: reportSexes.male,
    age: reportAges.aboveFour,
  },
  countMalesBelowFive: {
    sex: reportSexes.male,
    age: reportAges.belowFive,
  },
  countFemalesAtLeastFive: {
    sex: reportSexes.female,
    age: reportAges.aboveFour,
  },
  countFemalesBelowFive: {
    sex: reportSexes.female,
    age: reportAges.belowFive,
  },
  countUnspecifiedSexAndAge: {
    sex: reportSexes.unspecified,
    age: reportAges.unspecified,
  },
};

export const ReportErrorType = {
  formatError: "FormatError",
  healthRiskNotFound: "HealthRiskNotFound",
  tooLong: "TooLong",
  globalHealthRiskNotFound: "GlobalHealthRiskCodeNotFound",
  dataCollectorUsedCollectionPointFormat:
    "DataCollectorUsedCollectionPointFormat",
  collectionPointUsedDataCollectorFormat:
    "CollectionPointUsedDataCollectorFormat",
  collectionPointNonHumanHealthRisk: "CollectionPointNonHumanHealthRisk",
  singleReportNonHumanHealthRisk: "SingleReportNonHumanHealthRisk",
  aggregateReportNonHumanHealthRisk: "AggregateReportNonHumanHealthRisk",
  eventReportHumanHealthRisk: "EventReportHumanHealthRisk",
  genderAndAgeNonHumanHealthRisk: "GenderAndAgeNonHumanHealthRisk",
  gateway: "Gateway",
  other: "Other",
};

export const reportDetailedFormatErrors = [
  ReportErrorType.genderAndAgeNonHumanHealthRisk,
  ReportErrorType.singleReportNonHumanHealthRisk,
  ReportErrorType.eventReportHumanHealthRisk,
  ReportErrorType.dataCollectorUsedCollectionPointFormat,
  ReportErrorType.collectionPointUsedDataCollectorFormat,
  ReportErrorType.collectionPointNonHumanHealthRisk,
  ReportErrorType.aggregateReportNonHumanHealthRisk,
  ReportErrorType.tooLong,
];

export const ReportType = {
  single: "Single",
  aggregate: "Aggregate",
  event: "Event",
  dataCollectionPoint: "DataCollectionPoint",
};
