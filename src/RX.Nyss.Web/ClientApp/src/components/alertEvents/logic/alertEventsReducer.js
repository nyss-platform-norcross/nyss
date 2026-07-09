import * as actions from "./alertEventsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";

export function alertEventsReducer(state = initialState.alertEvents, action) {
  switch (action.type) {
    case actions.OPEN_ALERT_EVENT_LOG.REQUEST:
      return { ...state, logFetching: true };

    case actions.OPEN_ALERT_EVENT_LOG.SUCCESS:
      return { ...state, logFetching: false, logItems: action.data.logItems };

    case actions.OPEN_ALERT_EVENT_LOG.FAILURE:
      return { ...state, logFetching: false };

    case actions.GET_ALERT_EVENT_LOG.REQUEST:
      return { ...state, logFetching: true, logItems: null };

    case actions.GET_ALERT_EVENT_LOG.SUCCESS:
      return { ...state, logFetching: false, logItems: action.data.logItems };

    case actions.GET_ALERT_EVENT_LOG.FAILURE:
      return { ...state, logFetching: false };

    case actions.OPEN_ALERT_EVENT_CREATION.REQUEST:
      return { ...state, formFetching: true };

    case actions.OPEN_ALERT_EVENT_CREATION.SUCCESS:
      return {
        ...state,
        formFetching: false,
        eventTypes: action.alertEventTypes,
        eventSubtypes: action.alertEventSubtypes,
      };

    case actions.OPEN_ALERT_EVENT_CREATION.FAILURE:
      return { ...state, formFetching: false, formError: action.message };

    case actions.CREATE_ALERT_EVENT.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.CREATE_ALERT_EVENT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_ALERT_EVENT.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_ALERT_EVENT.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.EDIT_ALERT_EVENT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_ALERT_EVENT.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.DELETE_ALERT_EVENT.REQUEST:
      return {
        ...state,
        logRemoving: setProperty(
          state.logRemoving,
          action.alertEventLogId,
          true,
        ),
      };

    case actions.DELETE_ALERT_EVENT.SUCCESS:
      return {
        ...state,
        logRemoving: setProperty(
          state.logRemoving,
          action.alertEventLogId,
          undefined,
        ),
        listStale: true,
      };

    case actions.DELETE_ALERT_EVENT.FAILURE:
      return {
        ...state,
        logRemoving: setProperty(
          state.logRemoving,
          action.alertEventLogId,
          undefined,
        ),
      };

    default:
      return state;
  }
}
