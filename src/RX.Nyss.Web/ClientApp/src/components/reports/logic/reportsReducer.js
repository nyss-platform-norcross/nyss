import * as actions from "./reportsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function reportsReducer(state = initialState.reports, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.GET_REPORTS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_REPORTS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list, listStale: false };

    case actions.GET_REPORTS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    default:
      return state;
  }
};
