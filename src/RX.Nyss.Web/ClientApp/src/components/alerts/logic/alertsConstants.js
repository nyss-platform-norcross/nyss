import { action } from "../../../utils/actions";

export const OPEN_ALERTS_LIST = action("OPEN_ALERTS_LIST");
export const GET_ALERTS = action("GET_ALERTS");
export const OPEN_ALERTS_ASSESSMENT = action("OPEN_ALERTS_ASSESSMENT");
export const OPEN_ALERTS_LOGS = action("OPEN_ALERTS_LOGS");
export const ACCEPT_REPORT = action("ACCEPT_REPORT");
export const DISMISS_REPORT = action("DISMISS_REPORT");
export const ESCALATE_ALERT = action("ESCALATE_ALERT");
export const DISMISS_ALERT = action("DISMISS_ALERT");
export const CLOSE_ALERT = action("CLOSE_ALERT");

export const assessmentStatus = {
  escalated: "Escalated",
  closed: "Closed",
  dismissed: "Dismissed",
  rejected: "Rejected",
  toEscalate: "ToEscalate",
  toDismiss: "ToDismiss",
  open: "Open"
}

export const logType = {
  triggeredAlert: "TriggeredAlert",
  escalatedAlert: "EscalatedAlert",
  dismissedAlert: "DismissedAlert",
  closedAlert: "ClosedAlert",
  acceptedReport: "AcceptedReport",
  rejectedReport: "RejectedReport"
}

export const closeOptions = {
  dismissed: "Dismissed",
  actionTaken: "ActionTaken",
  other: "Other"
}