import * as actions from "./nationalSocietyReportsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function nationalSocietyReportsReducer(state = initialState.nationalSocietyReports, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.OPEN_NATIONAL_SOCIETY_REPORTS_LIST.INVOKE:
      return { ...state, listStale: action.nationalSocietyId !== state.listNationalSocietyId };

    case actions.OPEN_NATIONAL_SOCIETY_REPORTS_LIST.SUCCESS:
      return { ...state, listNationalSocietyId: action.nationalSocietyId };

    case actions.GET_NATIONAL_SOCIETY_REPORTS.REQUEST:
      return { ...state, paginatedListData: state.listStale ? null : state.paginatedListData, listFetching: true };

    case actions.GET_NATIONAL_SOCIETY_REPORTS.SUCCESS:
      return { ...state, listFetching: false, listStale: false, paginatedListData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows } };

    case actions.GET_NATIONAL_SOCIETY_REPORTS.FAILURE:
      return { ...state, listFetching: false, paginatedListData: null };

    default:
      return state;
  }
};
