import { push } from "connected-react-router";
import {
  OPEN_ALERTS_LIST, GET_ALERTS
} from "./alertsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/alerts`);

export const openList = {
  invoke: (projectId) => ({ type: OPEN_ALERTS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_ALERTS_LIST.REQUEST }),
  success: (projectId) => ({ type: OPEN_ALERTS_LIST.SUCCESS, projectId }),
  failure: (message) => ({ type: OPEN_ALERTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId, pageNumber) => ({ type: GET_ALERTS.INVOKE, projectId, pageNumber }),
  request: () => ({ type: GET_ALERTS.REQUEST }),
  success: (data, page, rowsPerPage, totalRows) => ({ type: GET_ALERTS.SUCCESS, data, page, rowsPerPage, totalRows }),
  failure: (message) => ({ type: GET_ALERTS.FAILURE, message })
};
