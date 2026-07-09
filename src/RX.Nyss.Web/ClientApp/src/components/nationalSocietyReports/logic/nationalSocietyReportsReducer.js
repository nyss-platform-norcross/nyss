import * as actions from "./nationalSocietyReportsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function nationalSocietyReportsReducer(
  state = initialState.nationalSocietyReports,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null };

    case actions.OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.INVOKE:
      return {
        ...state,
        correctReportsListStale:
          action.nationalSocietyId !== state.listNationalSocietyId,
      };

    case actions.OPEN_NATIONAL_SOCIETY_CORRECT_REPORTS_LIST.SUCCESS:
      return {
        ...state,
        listNationalSocietyId: action.nationalSocietyId,
        filtersData: action.filtersData,
      };

    case actions.OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.INVOKE:
      return {
        ...state,
        incorrectReportsListStale:
          state.incorrectReportsListStale ||
          action.nationalSocietyId !== state.listNationalSocietyId,
      };

    case actions.OPEN_NATIONAL_SOCIETY_INCORRECT_REPORTS_LIST.SUCCESS:
      return { ...state, listNationalSocietyId: action.nationalSocietyId };

    case actions.GET_NATIONAL_SOCIETY_CORRECT_REPORTS.REQUEST:
      return {
        ...state,
        correctReportsPaginatedListData: state.correctReportsListStale
          ? null
          : state.correctReportsPaginatedListData,
        listFetching: true,
      };

    case actions.GET_NATIONAL_SOCIETY_CORRECT_REPORTS.SUCCESS:
      return {
        ...state,
        correctReportsFilters: action.filters,
        correctReportsSorting: action.sorting,
        listFetching: false,
        correctReportsListStale: false,
        correctReportsPaginatedListData: {
          data: action.data,
          page: action.page,
          rowsPerPage: action.rowsPerPage,
          totalRows: action.totalRows,
        },
      };

    case actions.GET_NATIONAL_SOCIETY_CORRECT_REPORTS.FAILURE:
      return {
        ...state,
        listFetching: false,
        correctReportsPaginatedListData: null,
      };

    case actions.GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.REQUEST:
      return {
        ...state,
        incorrectReportsPaginatedListData: state.incorrectReportsListStale
          ? null
          : state.incorrectReportsPaginatedListData,
        listFetching: true,
      };

    case actions.GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.SUCCESS:
      return {
        ...state,
        incorrectReportsFilters: action.filters,
        incorrectReportsSorting: action.sorting,
        listFetching: false,
        incorrectReportsListStale: false,
        incorrectReportsPaginatedListData: {
          data: action.data,
          page: action.page,
          rowsPerPage: action.rowsPerPage,
          totalRows: action.totalRows,
        },
      };

    case actions.GET_NATIONAL_SOCIETY_INCORRECT_REPORTS.FAILURE:
      return {
        ...state,
        listFetching: false,
        incorrectReportsPaginatedListData: null,
      };

    default:
      return state;
  }
}
