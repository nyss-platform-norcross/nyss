import { push } from "connected-react-router";
import {
  OPEN_CORRECT_REPORTS_LIST,
  OPEN_REPORT_EDITION,
  EDIT_REPORT,
  EXPORT_TO_EXCEL,
  EXPORT_TO_CSV,
  OPEN_SEND_REPORT,
  ACCEPT_REPORT,
  DISMISS_REPORT,
  OPEN_INCORRECT_REPORTS_LIST,
  GET_INCORRECT_REPORTS,
  GET_CORRECT_REPORTS,
  TRACK_REPORT_EXPORT,
  MARK_AS_CORRECTED,
  MARK_AS_NOT_CORRECTED,
} from "./reportsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/reports`);
export const goToEditing = (projectId, reportId) =>
  push(`/projects/${projectId}/reports/${reportId}/edit`);
export const goToAlert = (projectId, alertId) =>
  push(`/projects/${projectId}/alerts/${alertId}/assess`);

export const openCorrectReportsList = {
  invoke: (projectId) => ({
    type: OPEN_CORRECT_REPORTS_LIST.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_CORRECT_REPORTS_LIST.REQUEST }),
  success: (projectId, filtersData) => ({
    type: OPEN_CORRECT_REPORTS_LIST.SUCCESS,
    projectId,
    filtersData,
  }),
  failure: (message) => ({ type: OPEN_CORRECT_REPORTS_LIST.FAILURE, message }),
};

export const openIncorrectReportsList = {
  invoke: (projectId) => ({
    type: OPEN_INCORRECT_REPORTS_LIST.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_INCORRECT_REPORTS_LIST.REQUEST }),
  success: (projectId, filtersData) => ({
    type: OPEN_INCORRECT_REPORTS_LIST.SUCCESS,
    projectId,
    filtersData,
  }),
  failure: (message) => ({
    type: OPEN_INCORRECT_REPORTS_LIST.FAILURE,
    message,
  }),
};

export const getCorrectList = {
  invoke: (projectId, pageNumber, filters, sorting) => ({
    type: GET_CORRECT_REPORTS.INVOKE,
    projectId,
    pageNumber,
    filters,
    sorting,
  }),
  request: () => ({ type: GET_CORRECT_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({
    type: GET_CORRECT_REPORTS.SUCCESS,
    data,
    page,
    rowsPerPage,
    totalRows,
    filters,
    sorting,
  }),
  failure: (message) => ({ type: GET_CORRECT_REPORTS.FAILURE, message }),
};

export const getIncorrectList = {
  invoke: (projectId, pageNumber, filters, sorting) => ({
    type: GET_INCORRECT_REPORTS.INVOKE,
    projectId,
    pageNumber,
    filters,
    sorting,
  }),
  request: () => ({ type: GET_INCORRECT_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filters, sorting) => ({
    type: GET_INCORRECT_REPORTS.SUCCESS,
    data,
    page,
    rowsPerPage,
    totalRows,
    filters,
    sorting,
  }),
  failure: (message) => ({ type: GET_INCORRECT_REPORTS.FAILURE, message }),
};

export const openEdition = {
  invoke: (projectId, reportId) => ({
    type: OPEN_REPORT_EDITION.INVOKE,
    projectId,
    reportId,
  }),
  request: () => ({ type: OPEN_REPORT_EDITION.REQUEST }),
  success: (data, healthRisks, dataCollectors) => ({
    type: OPEN_REPORT_EDITION.SUCCESS,
    data,
    healthRisks,
    dataCollectors,
  }),
  failure: (message) => ({ type: OPEN_REPORT_EDITION.FAILURE, message }),
};

export const edit = {
  invoke: (projectId, reportId, data) => ({
    type: EDIT_REPORT.INVOKE,
    projectId,
    reportId,
    data,
  }),
  request: () => ({ type: EDIT_REPORT.REQUEST }),
  success: () => ({ type: EDIT_REPORT.SUCCESS }),
  failure: (message) => ({
    type: EDIT_REPORT.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const exportToExcel = {
  invoke: (projectId, filters, sorting) => ({
    type: EXPORT_TO_EXCEL.INVOKE,
    projectId,
    filters,
    sorting,
  }),
  request: () => ({ type: EXPORT_TO_EXCEL.REQUEST }),
  success: () => ({ type: EXPORT_TO_EXCEL.SUCCESS }),
  failure: (message) => ({ type: EXPORT_TO_EXCEL.FAILURE, message }),
};

export const exportToCsv = {
  invoke: (projectId, filters, sorting) => ({
    type: EXPORT_TO_CSV.INVOKE,
    projectId,
    filters,
    sorting,
  }),
  request: () => ({ type: EXPORT_TO_CSV.REQUEST }),
  success: () => ({ type: EXPORT_TO_CSV.SUCCESS }),
  failure: (message) => ({ type: EXPORT_TO_CSV.FAILURE, message }),
};

export const openSendReport = {
  invoke: (projectId) => ({ type: OPEN_SEND_REPORT.INVOKE, projectId }),
  request: () => ({ type: OPEN_SEND_REPORT.REQUEST }),
  success: (dataCollectors, formData) => ({
    type: OPEN_SEND_REPORT.SUCCESS,
    dataCollectors,
    formData,
  }),
  failure: (message) => ({ type: OPEN_SEND_REPORT.FAILURE, message }),
};

export const acceptReport = {
  invoke: (reportId) => ({ type: ACCEPT_REPORT.INVOKE, reportId }),
  request: () => ({ type: ACCEPT_REPORT.REQUEST }),
  success: () => ({ type: ACCEPT_REPORT.SUCCESS }),
  failure: (message) => ({ type: ACCEPT_REPORT.FAILURE, message }),
};

export const dismissReport = {
  invoke: (reportId) => ({ type: DISMISS_REPORT.INVOKE, reportId }),
  request: () => ({ type: DISMISS_REPORT.REQUEST }),
  success: () => ({ type: DISMISS_REPORT.SUCCESS }),
  failure: (message) => ({ type: DISMISS_REPORT.FAILURE, message }),
};

export const markAsCorrected = {
  invoke: (reportId) => ({ type: MARK_AS_CORRECTED.INVOKE, reportId }),
  request: () => ({ type: MARK_AS_CORRECTED.REQUEST }),
  success: (reportId) => ({ type: MARK_AS_CORRECTED.SUCCESS, reportId }),
  failure: (message) => ({ type: MARK_AS_CORRECTED.FAILURE, message }),
};

export const markAsNotCorrected = {
  invoke: (reportId) => ({ type: MARK_AS_NOT_CORRECTED.INVOKE, reportId }),
  request: () => ({ type: MARK_AS_NOT_CORRECTED.REQUEST }),
  success: (reportId) => ({ type: MARK_AS_NOT_CORRECTED.SUCCESS, reportId }),
  failure: (message) => ({ type: MARK_AS_NOT_CORRECTED.FAILURE, message }),
};

export const trackReportExport = (page, fileType, projectId) => ({
  type: TRACK_REPORT_EXPORT.INVOKE,
  page,
  fileType,
  projectId,
});
