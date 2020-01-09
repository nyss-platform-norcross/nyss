import * as actions from "./reportsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";
import { setProperty } from "../../../utils/immutable";

export function reportsReducer(state = initialState.reports, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.OPEN_REPORTS_LIST.INVOKE:
      return { ...state, listStale: action.projectId !== state.listProjectId };

    case actions.OPEN_REPORTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId };

    case actions.GET_REPORTS.REQUEST:
      return { ...state, paginatedListData: state.listStale ? null : state.paginatedListData, listFetching: true };

    case actions.GET_REPORTS.SUCCESS:
      return { ...state, filter: action.filter, listFetching: false, listStale: false, paginatedListData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows } };

    case actions.GET_REPORTS.FAILURE:
      return { ...state, listFetching: false, paginatedListData: null };
      
    case actions.MARK_AS_ERROR.REQUEST:
    return { ...state, markingAsError: true };

    case actions.MARK_AS_ERROR.SUCCESS:
      return { ...state, markingAsError: false };

    case actions.MARK_AS_ERROR.FAILURE:
      return { ...state, markingAsError: false, message: action.message };
            
    case actions.UNMARK_AS_ERROR.REQUEST:
      return { ...state, markingAsError: true };
  
      case actions.UNMARK_AS_ERROR.SUCCESS:
        return { ...state, markingAsError: false };
  
      case actions.UNMARK_AS_ERROR.FAILURE:
        return { ...state, markingAsError: false, message: action.message };
        
    default:
      return state;
  }
};
