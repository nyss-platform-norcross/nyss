import * as actions from "./alertsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function alertsReducer(state = initialState.alerts, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.OPEN_ALERTS_LIST.INVOKE:
      return { ...state, listStale: action.projectId !== state.listProjectId };

    case actions.OPEN_ALERTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId };

    case actions.GET_ALERTS.REQUEST:
      return { ...state, listData: state.listStale ? null : state.listData, listFetching: true };

    case actions.GET_ALERTS.SUCCESS:
      return { ...state, listFetching: false, listStale: false, listData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows} };

    case actions.GET_ALERTS.FAILURE:
      return { ...state, listFetching: false, listData: null };

    default:
      return state;
  }
};
