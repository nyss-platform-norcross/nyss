import * as actions from "./reportsConstants";
import * as projectsActions from "../../projects/logic/projectsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function reportsReducer(state = initialState.reports, action) {
  switch (action.type) {
    case projectsActions.CLOSE_PROJECT.SUCCESS:
      return { ...state, listStale: true, listProjectId: null };

    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.OPEN_CORRECT_REPORTS_LIST.INVOKE:
      return { ...state, correctReportsListStale: state.correctReportsListStale || action.projectId !== state.listProjectId };

    case actions.OPEN_CORRECT_REPORTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId, filtersData: action.filtersData };

    case actions.OPEN_INCORRECT_REPORTS_LIST.INVOKE:
      return { ...state, incorrectReportsListStale: state.incorrectReportsListStale || action.projectId !== state.listProjectId };

    case actions.OPEN_INCORRECT_REPORTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId };

    case actions.GET_CORRECT_REPORTS.REQUEST:
      return { ...state, correctReportsPaginatedListData: state.correctReportsListStale ? null : state.correctReportsPaginatedListData, listFetching: true };

    case actions.GET_CORRECT_REPORTS.SUCCESS:
      return { ...state, correctReportsFilters: action.filters, correctReportsSorting: action.sorting, listFetching: false, correctReportsListStale: false, correctReportsPaginatedListData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows } };

    case actions.GET_CORRECT_REPORTS.FAILURE:
      return { ...state, listFetching: false, correctReportsPaginatedListData: null };

    case actions.GET_INCORRECT_REPORTS.REQUEST:
      return { ...state, incorrectReportsPaginatedListData: state.incorrectReportsListStale ? null : state.incorrectReportsPaginatedListData, listFetching: true };

    case actions.GET_INCORRECT_REPORTS.SUCCESS:
      return { ...state, incorrectReportsFilters: action.filters, incorrectReportsSorting: action.sorting, listFetching: false, incorrectReportsListStale: false, incorrectReportsPaginatedListData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows } };

    case actions.GET_INCORRECT_REPORTS.FAILURE:
      return { ...state, listFetching: false, incorrectReportsPaginatedListData: null };

    case actions.OPEN_REPORT_EDITION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_REPORT_EDITION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_REPORT_EDITION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data, editReport: { formHealthRisks: action.healthRisks, formDataCollectors: action.dataCollectors } };

    case actions.OPEN_REPORT_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.EDIT_REPORT.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_REPORT.SUCCESS:
      return { ...state, formSaving: false, correctReportsListStale: true, incorrectReportsListStale: true };

    case actions.EDIT_REPORT.FAILURE:
      return { ...state, formSaving: false };

    case actions.MARK_AS_ERROR.REQUEST:
      return { ...state, markingAsError: true };

    case actions.MARK_AS_ERROR.SUCCESS:
      return { ...state, markingAsError: false };

    case actions.MARK_AS_ERROR.FAILURE:
      return { ...state, markingAsError: false, message: action.message };

    case actions.OPEN_SEND_REPORT.REQUEST:
      return { ...state, formFetching: true };

    case actions.OPEN_SEND_REPORT.SUCCESS:
      return { ...state, formFetching: false, sendReport: { dataCollectors: action.dataCollectors, formData: action.formData } };

    case actions.OPEN_SEND_REPORT.FAILURE:
      return { ...state, formFetching: false, message: action.message };

    case actions.SEND_REPORT.REQUEST:
      return { ...state, formSaving: true };

    case actions.SEND_REPORT.SUCCESS:
      return { ...state, formSaving: false };

    case actions.SEND_REPORT.FAILURE:
      return { ...state, formSaving: false };

    case actions.ACCEPT_REPORT.REQUEST:
      return { ...state, listFetching: true };

    case actions.ACCEPT_REPORT.SUCCESS:
      return { ...state, listFetching: false };

    case actions.ACCEPT_REPORT.FAILURE:
      return { ...state, listFetching: false };

    case actions.DISMISS_REPORT.REQUEST:
      return { ...state, listFetching: true };

    case actions.DISMISS_REPORT.SUCCESS:
      return { ...state, listFetching: false };

    case actions.DISMISS_REPORT.FAILURE:
      return { ...state, listFetching: false };

    default:
      return state;
  }
};
