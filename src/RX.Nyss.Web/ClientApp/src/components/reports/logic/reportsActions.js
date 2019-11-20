import { push } from "connected-react-router";
import {
  OPEN_REPORTS_LIST, GET_REPORTS
} from "./reportsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/reports`);

export const openList = {
  invoke: (projectId) => ({ type: OPEN_REPORTS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_REPORTS_LIST.REQUEST }),
  success: () => ({ type: OPEN_REPORTS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_REPORTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId) => ({ type: GET_REPORTS.INVOKE, projectId }),
  request: () => ({ type: GET_REPORTS.REQUEST }),
  success: (list) => ({ type: GET_REPORTS.SUCCESS, list }),
  failure: (message) => ({ type: GET_REPORTS.FAILURE, message })
};
