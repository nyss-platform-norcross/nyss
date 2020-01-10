import { push } from "connected-react-router";
import {
  OPEN_REPORTS_LIST, GET_REPORTS,
  EXPORT_TO_EXCEL, MARK_AS_ERROR
} from "./reportsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/reports`);

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

export const exportToExcel = {
  invoke: (projectId, filters, sorting) => ({ type: EXPORT_TO_EXCEL.INVOKE, projectId, filters, sorting }),
  request: () => ({ type: EXPORT_TO_EXCEL.REQUEST }),
  success: () => ({ type: EXPORT_TO_EXCEL.SUCCESS }),
  failure: (message) => ({ type: EXPORT_TO_EXCEL.FAILURE, message })
};

export const markAsError = {
  invoke: (reportId) => ({ type: MARK_AS_ERROR.INVOKE, reportId }),
  request: () => ({ type: MARK_AS_ERROR.REQUEST }),
  success: () => ({ type: MARK_AS_ERROR.SUCCESS }),
  failure: (message) => ({ type: MARK_AS_ERROR.FAILURE, message })
};
