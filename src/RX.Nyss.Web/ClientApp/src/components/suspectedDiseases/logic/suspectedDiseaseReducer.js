import * as actions from "./suspectedDiseaseConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function suspectedDiseaseReducer(
  state = initialState.suspectedDiseases,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE:
      return { ...state, formData: null, formError: null };

    case actions.GET_SUSPECTED_DISEASE.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_SUSPECTED_DISEASE.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_SUSPECTED_DISEASE.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_EDITION_SUSPECTED_DISEASE.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_SUSPECTED_DISEASE.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_SUSPECTED_DISEASE.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_EDITION_SUSPECTED_DISEASE.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_SUSPECTED_DISEASE.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_SUSPECTED_DISEASE.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.CREATE_SUSPECTED_DISEASE.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_SUSPECTED_DISEASE.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.EDIT_SUSPECTED_DISEASE.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.EDIT_SUSPECTED_DISEASE.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.REMOVE_SUSPECTED_DISEASE.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_SUSPECTED_DISEASE.SUCCESS:
    case actions.REMOVE_SUSPECTED_DISEASE.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    default:
      return state;
  }
}
