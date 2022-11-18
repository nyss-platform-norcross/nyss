import * as actions from "./alertsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";
import { assignInArray } from "../../../utils/immutable";

const updateReport = (reports, reportId, changes) =>
  assignInArray(reports, r => r.id === reportId, item => ({ ...item, ...changes }));

export function alertsReducer(state = initialState.alerts, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }


    case actions.OPEN_ALERTS_LIST.INVOKE:
      return { ...state };

    case actions.OPEN_ALERTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId, filtersData: action.filtersData };


    case actions.GET_ALERTS.REQUEST:
      return { ...state, listData: state.listData, listFetching: true };

    case actions.GET_ALERTS.SUCCESS:
      return { ...state, listFetching: false, listData: { data: action.data, page: action.page, rowsPerPage: action.rowsPerPage, totalRows: action.totalRows }, filters: action.filters };

    case actions.GET_ALERTS.FAILURE:
      return { ...state, listFetching: false, listData: null };


    case actions.OPEN_ALERTS_ASSESSMENT.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_ALERTS_ASSESSMENT.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_ALERTS_ASSESSMENT.FAILURE:
      return { ...state, formFetching: false };


    case actions.ACCEPT_REPORT.REQUEST:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isAccepting: true })
        }
      };

    case actions.ACCEPT_REPORT.SUCCESS:
      return {
        ...state,
        formData: {
          ...state.formData,
          assessmentStatus: action.assessmentStatus,
          reports: updateReport(state.formData.reports, action.reportId, { isAccepting: false, status: "Accepted" })
        }
      };

    case actions.ACCEPT_REPORT.FAILURE:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isAccepting: false })
        }
      };


    case actions.DISMISS_REPORT.REQUEST:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isDismissing: true })
        }
      };

    case actions.DISMISS_REPORT.SUCCESS:
      return {
        ...state,
        formData: {
          ...state.formData,
          assessmentStatus: action.assessmentStatus,
          reports: updateReport(state.formData.reports, action.reportId, { isDismissing: false, status: "Rejected" })
        }
      };

    case actions.DISMISS_REPORT.FAILURE:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isDismissing: false })
        }
      };


    case actions.RESET_REPORT.REQUEST:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isResetting: true })
        }
      };

    case actions.RESET_REPORT.SUCCESS:
      return {
        ...state,
        formData: {
          ...state.formData,
          assessmentStatus: action.assessmentStatus,
          reports: updateReport(state.formData.reports, action.reportId, { isResetting: false, status: "Pending" })
        }
      };

    case actions.RESET_REPORT.FAILURE:
      return {
        ...state,
        formData: {
          ...state.formData,
          reports: updateReport(state.formData.reports, action.reportId, { isResetting: false })
        }
      };


    case actions.ESCALATE_ALERT.REQUEST:
      return { ...state, formEscalating: true };

    case actions.ESCALATE_ALERT.SUCCESS:
      return { ...state, formEscalating: false, formData: { ...state.formData, assessmentStatus: actions.assessmentStatus.escalated } };

    case actions.ESCALATE_ALERT.FAILURE:
      return { ...state, formEscalating: false };


    case actions.DISMISS_ALERT.REQUEST:
      return { ...state, formDismissing: true };

    case actions.DISMISS_ALERT.SUCCESS:
      return { ...state, formDismissing: false, formData: { ...state.formData, assessmentStatus: actions.assessmentStatus.dismissed } };

    case actions.DISMISS_ALERT.FAILURE:
      return { ...state, formDismissing: false };


    case actions.CLOSE_ALERT.REQUEST:
      return { ...state, formClosing: true };

    case actions.CLOSE_ALERT.SUCCESS:
      return { ...state, formClosing: false, formData: { ...state.formData, assessmentStatus: actions.assessmentStatus.closed } };

    case actions.CLOSE_ALERT.FAILURE:
      return { ...state, formClosing: false };


    case actions.FETCH_RECIPIENTS.REQUEST:
      return { ...state, isFetchingRecipients: true, notificationEmails: [], notificationPhoneNumbers: [] };

    case actions.FETCH_RECIPIENTS.SUCCESS:
      return { ...state, isFetchingRecipients: false, notificationEmails: action.data.emails, notificationPhoneNumbers: action.data.phoneNumbers };

    case actions.FETCH_RECIPIENTS.FAILURE:
      return { ...state, isFetchingRecipients: false, notificationEmails: [], notificationPhoneNumbers: [] };


    case actions.VALIDATE_EIDSR.REQUEST:
      return { ...state, isLoadingValidateEidsr: true };

    case actions.VALIDATE_EIDSR.SUCCESS:
      return { ...state, isLoadingValidateEidsr: false, formData: { ...state.formData, validateEidsrResult: action.data } };

    case actions.VALIDATE_EIDSR.FAILURE:
      return { ...state, isLoadingValidateEidsr: false };

    default:
      return state;
  }
};
