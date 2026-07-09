import { action } from "../../../utils/actions";

export const OPEN_ALERTS_LIST = action("OPEN_ALERTS_LIST");
export const GET_ALERTS = action("GET_ALERTS");
export const OPEN_ALERTS_ASSESSMENT = action("OPEN_ALERTS_ASSESSMENT");
export const ACCEPT_REPORT = action("ACCEPT_REPORT");
export const DISMISS_REPORT = action("DISMISS_REPORT");
export const RESET_REPORT = action("RESET_REPORT");
export const ESCALATE_ALERT = action("ESCALATE_ALERT");
export const DISMISS_ALERT = action("DISMISS_ALERT");
export const CLOSE_ALERT = action("CLOSE_ALERT");
export const FETCH_RECIPIENTS = action("FETCH_RECIPIENTS");
export const EXPORT_ALERTS = action("EXPORT_ALERTS");
export const VALIDATE_EIDSR = action("VALIDATE_EIDSR");

export const assessmentStatus = {
  escalated: "Escalated",
  closed: "Closed",
  dismissed: "Dismissed",
  rejected: "Rejected",
  toEscalate: "ToEscalate",
  toDismiss: "ToDismiss",
  open: "Open",
};

export const escalatedOutcomes = {
  dismissed: "Dismissed",
  actionTaken: "ActionTaken",
  other: "Other",
};

export const alertStatusFilters = {
  all: "All",
  open: "Open",
  escalated: "Escalated",
  dismissed: "Dismissed",
  closed: "Closed",
};

export const alertStatus = {
  open: "Open",
  escalated: "Escalated",
  dismissed: "Dismissed",
  closed: "Closed",
};

export const dateFilter = "DateFilter";
export const timeTriggeredColumn = "TimeTriggered";
export const timeOfLastReportColumn = "TimeOfLastReport";
export const statusColumn = "Status";
