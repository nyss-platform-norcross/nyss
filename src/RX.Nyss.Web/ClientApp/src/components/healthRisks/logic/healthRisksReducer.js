import * as actions from "./healthRisksConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function healthRisksReducer(state = initialState.healthRisks, action) {
  switch (action.type) {
    case LOCATION_CHANGE:
      return { ...state, formData: null, formError: null };

    case actions.GET_HEALTH_RISKS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_HEALTH_RISKS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_HEALTH_RISKS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_EDITION_HEALTH_RISK.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_HEALTH_RISK.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_HEALTH_RISK.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_EDITION_HEALTH_RISK.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_CREATION_HEALTH_RISK.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_CREATION_HEALTH_RISK.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_CREATION_HEALTH_RISK.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_CREATION_HEALTH_RISK.FAILURE:
      return { ...state, formFetching: false, formError: action.error };

    case actions.CREATE_HEALTH_RISK.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_HEALTH_RISK.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.CREATE_HEALTH_RISK.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_HEALTH_RISK.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.EDIT_HEALTH_RISK.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.EDIT_HEALTH_RISK.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.REMOVE_HEALTH_RISK.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_HEALTH_RISK.SUCCESS:
    case actions.REMOVE_HEALTH_RISK.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    default:
      return state;
  }
}
