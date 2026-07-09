import * as actions from "./projectAlertRecipientsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function projectAlertRecipientsReducer(
  state = initialState.projectAlertRecipients,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.OPEN_ALERT_RECIPIENTS_LIST.INVOKE:
      return {
        ...state,
        listStale: state.listStale || action.projectId !== state.listProjectId,
      };

    case actions.OPEN_ALERT_RECIPIENTS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId };

    case actions.GET_ALERT_RECIPIENTS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_ALERT_RECIPIENTS.SUCCESS:
      return {
        ...state,
        listFetching: false,
        listData: action.list,
        listStale: false,
      };

    case actions.GET_ALERT_RECIPIENTS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_ALERT_RECIPIENT_CREATION.SUCCESS:
      return { ...state, formData: action.formData };

    case actions.OPEN_ALERT_RECIPIENT_EDITION.SUCCESS:
      return {
        ...state,
        recipient: action.recipient,
        formData: action.formData,
      };

    case actions.CREATE_ALERT_RECIPIENT.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_ALERT_RECIPIENT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_ALERT_RECIPIENT.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.EDIT_ALERT_RECIPIENT.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_ALERT_RECIPIENT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_ALERT_RECIPIENT.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.REMOVE_ALERT_RECIPIENT.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_ALERT_RECIPIENT.SUCCESS:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
        listStale: true,
      };

    case actions.REMOVE_ALERT_RECIPIENT.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    default:
      return state;
  }
}
