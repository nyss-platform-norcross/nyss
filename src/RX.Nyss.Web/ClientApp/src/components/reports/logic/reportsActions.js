import { push } from "connected-react-router";
import {
  OPEN_REPORTS_LIST, GET_REPORTS
} from "./reportsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/reports`);

export const openList = {
  invoke: (projectId) => ({ type: OPEN_REPORTS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_REPORTS_LIST.REQUEST }),
  success: (projectId) => ({ type: OPEN_REPORTS_LIST.SUCCESS, projectId }),
  failure: (message) => ({ type: OPEN_REPORTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId, pageNumber, reportListFilter) => ({ type: GET_REPORTS.INVOKE, projectId, pageNumber, reportListFilter }),
  request: () => ({ type: GET_REPORTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows, filter) => ({ type: GET_REPORTS.SUCCESS, data, page, rowsPerPage, totalRows, filter }),
  failure: (message) => ({ type: GET_REPORTS.FAILURE, message })
};
