import { action } from "../../../utils/actions";

export const OPEN_REPORTS_LIST = action("OPEN_REPORTS_LIST");
export const GET_REPORTS = action("GET_REPORTS");
export const OPEN_REPORT_EDITION = action("OPEN_REPORT_EDITION");
export const EDIT_REPORT = action("EDIT_REPORT");
export const EXPORT_TO_CSV = action("EXPORT_TO_CSV");
export const EXPORT_TO_EXCEL = action("EXPORT_TO_EXCEL");
export const MARK_AS_ERROR = action("MARK_AS_ERROR");
export const OPEN_SEND_REPORT = action("OPEN_SEND_REPORT");
export const SEND_REPORT = action("SEND_REPORT");
export const ACCEPT_REPORT = action("ACCEPT_REPORT_IN_LIST");
export const DISMISS_REPORT = action("DISMISS_REPORT_IN_LIST");

export const DateColumnName = "date";

export const reportStatus = {
  new: 'New',
  pending: 'Pending',
  accepted: 'Accepted',
  rejected: 'Rejected',
  closed: 'Closed'
}