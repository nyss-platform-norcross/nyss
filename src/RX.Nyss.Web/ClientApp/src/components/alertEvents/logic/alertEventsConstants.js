import { action } from "../../../utils/actions";

export const logType = {
  triggeredAlert: "TriggeredAlert",
  escalatedAlert: "EscalatedAlert",
  dismissedAlert: "DismissedAlert",
  closedAlert: "ClosedAlert",
  acceptedReport: "AcceptedReport",
  rejectedReport: "RejectedReport",
};

export const OPEN_ALERT_EVENT_LOG = action("OPEN_ALERT_EVENT_LOG");
export const GET_ALERT_EVENT_LOG = action("GET_ALERT_EVENT_LOG");
export const OPEN_ALERT_EVENT_CREATION = action("OPEN_ALERT_EVENT_CREATION");
export const CREATE_ALERT_EVENT = action("CREATE_ALERT_EVENT");
export const EDIT_ALERT_EVENT = action("EDIT_ALERT_EVENT");
export const DELETE_ALERT_EVENT = action("DELETE_ALERT_EVENT");
