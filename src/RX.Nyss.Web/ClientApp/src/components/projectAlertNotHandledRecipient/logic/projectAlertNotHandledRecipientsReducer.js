import * as actions from "./projectAlertNotHandledRecipientsConstants";
import { initialState } from "../../../initialState";
import { LOCATION_CHANGE } from "connected-react-router";

export function projectAlertNotHandledRecipientsReducer(
  state = initialState.projectAlertNotHandledRecipients,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.OPEN_ALERT_NOT_HANDLED_RECIPIENTS.INVOKE:
      return {
        ...state,
        listStale: state.listStale || action.projectId !== state.projectId,
      };

    case actions.OPEN_ALERT_NOT_HANDLED_RECIPIENTS.SUCCESS:
      return { ...state, projectId: action.projectId };

    case actions.GET_ALERT_NOT_HANDLED_RECIPIENTS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_ALERT_NOT_HANDLED_RECIPIENTS.SUCCESS:
      return {
        ...state,
        listFetching: false,
        listData: action.recipients,
        listStale: false,
      };

    case actions.GET_ALERT_NOT_HANDLED_RECIPIENTS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.CREATE_ALERT_NOT_HANDLED_RECIPIENT.REQUEST:
      return { ...state };

    case actions.CREATE_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS:
      return { ...state, listStale: true };

    case actions.CREATE_ALERT_NOT_HANDLED_RECIPIENT.FAILURE:
      return { ...state };

    case actions.EDIT_ALERT_NOT_HANDLED_RECIPIENT.REQUEST:
      return { ...state, saving: true };

    case actions.EDIT_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS:
      return { ...state, saving: false, listStale: true };

    case actions.EDIT_ALERT_NOT_HANDLED_RECIPIENT.FAILURE:
      return { ...state, saving: false };

    case actions.REMOVE_ALERT_NOT_HANDLED_RECIPIENT.REQUEST:
      return { ...state, saving: true };

    case actions.REMOVE_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS:
      return { ...state, saving: false, listStale: true };

    case actions.REMOVE_ALERT_NOT_HANDLED_RECIPIENT.FAILURE:
      return { ...state, saving: false };

    case actions.GET_ALERT_NOT_HANDLED_FORM_DATA.REQUEST:
      return { ...state, formDataFetching: true };

    case actions.GET_ALERT_NOT_HANDLED_FORM_DATA.SUCCESS:
      return { ...state, formDataFetching: false, users: action.users };

    case actions.GET_ALERT_NOT_HANDLED_FORM_DATA.FAILURE:
      return { ...state, formDataFetching: false, users: [] };

    default:
      return state;
  }
}
