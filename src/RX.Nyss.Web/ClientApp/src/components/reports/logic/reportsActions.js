import { push } from "connected-react-router";
import {
  OPEN_REPORTS_LIST, GET_REPORTS,
  OPEN_REPORT_EDITION, EDIT_REPORT,
  EXPORT_TO_EXCEL, MARK_AS_ERROR, EXPORT_TO_CSV, SEND_REPORT, OPEN_SEND_REPORT, ACCEPT_REPORT, DISMISS_REPORT
} from "./reportsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/reports`);
export const goToEdition = (projectId, reportId) => push(`/projects/${projectId}/reports/${reportId}/edit`);
export const goToAlert = (projectId, alertId) => push(`/projects/${projectId}/alerts/${alertId}/assess`);

export const openList = {
  invoke: (projectId) => ({ type: OPEN_REPORTS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_REPORTS_LIST.REQUEST }),
  success: (projectId, filtersData) => ({ type: OPEN_REPORTS_LIST.SUCCESS, projectId, filtersData }),
  failure: (message) => ({ type: OPEN_REPORTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId, pageNumber, filters, sorting) => ({ type: GET_REPORTS.INVOKE, projectId, pageNumber, filters, sorting }),
  request: () => ({ type: GET_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({ type: GET_REPORTS.SUCCESS, data, page, rowsPerPage, totalRows, filters, sorting }),
  failure: (message) => ({ type: GET_REPORTS.FAILURE, message })
};

export const openEdition = {
  invoke: (projectId, reportId) => ({ type: OPEN_REPORT_EDITION.INVOKE, projectId, reportId }),
  request: () => ({ type: OPEN_REPORT_EDITION.REQUEST }),
  success: (data, healthRisks) => ({ type: OPEN_REPORT_EDITION.SUCCESS, data, healthRisks }),
  failure: (message) => ({ type: OPEN_REPORT_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (projectId, reportId, data) => ({ type: EDIT_REPORT.INVOKE, projectId, reportId, data }),
  request: () => ({ type: EDIT_REPORT.REQUEST }),
  success: () => ({ type: EDIT_REPORT.SUCCESS }),
  failure: (message) => ({ type: EDIT_REPORT.FAILURE, message, suppressPopup: true })
};

export const exportToExcel = {
  invoke: (projectId, filters, sorting) => ({ type: EXPORT_TO_EXCEL.INVOKE, projectId, filters, sorting }),
  request: () => ({ type: EXPORT_TO_EXCEL.REQUEST }),
  success: () => ({ type: EXPORT_TO_EXCEL.SUCCESS }),
  failure: (message) => ({ type: EXPORT_TO_EXCEL.FAILURE, message })
};

export const exportToCsv = {
  invoke: (projectId, filters, sorting) => ({ type: EXPORT_TO_CSV.INVOKE, projectId, filters, sorting }),
  request: () => ({ type: EXPORT_TO_CSV.REQUEST }),
  success: () => ({ type: EXPORT_TO_CSV.SUCCESS }),
  failure: (message) => ({ type: EXPORT_TO_CSV.FAILURE, message })
};

export const markAsError = {
  invoke: (reportId) => ({ type: MARK_AS_ERROR.INVOKE, reportId }),
  request: () => ({ type: MARK_AS_ERROR.REQUEST }),
  success: () => ({ type: MARK_AS_ERROR.SUCCESS }),
  failure: (message) => ({ type: MARK_AS_ERROR.FAILURE, message })
};

export const openSendReport = {
  invoke: (projectId) => ({ type: OPEN_SEND_REPORT.INVOKE, projectId }),
  request: () => ({ type: OPEN_SEND_REPORT.REQUEST }),
  success: (dataCollectors, formData) => ({ type: OPEN_SEND_REPORT.SUCCESS, dataCollectors, formData }),
  failure: (message) => ({ type: OPEN_SEND_REPORT.FAILURE, message })
}

export const sendReport = {
  invoke: (report) => ({ type: SEND_REPORT.INVOKE, report }),
  request: () => ({ type: SEND_REPORT.REQUEST }),
  success: () => ({ type: SEND_REPORT.SUCCESS }),
  failure: (message) => ({ type: SEND_REPORT.FAILURE, message })
}

export const acceptReport = {
  invoke: (reportId) => ({ type: ACCEPT_REPORT.INVOKE, reportId }),
  request: () => ({ type: ACCEPT_REPORT.REQUEST }),
  success: () => ({ type: ACCEPT_REPORT.SUCCESS }),
  failure: (message) => ({ type: ACCEPT_REPORT.FAILURE, message })
}

export const dismissReport = {
  invoke: (reportId) => ({ type: DISMISS_REPORT.INVOKE, reportId }),
  request: () => ({ type: DISMISS_REPORT.REQUEST }),
  success: () => ({ type: DISMISS_REPORT.SUCCESS }),
  failure: (message) => ({ type: DISMISS_REPORT.FAILURE, message })
}