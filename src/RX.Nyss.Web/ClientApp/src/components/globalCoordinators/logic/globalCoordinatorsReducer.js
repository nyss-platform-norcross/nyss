import * as actions from "./globalCoordinatorsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function globalCoordinatorsReducer(
  state = initialState.globalCoordinators,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.GET_GLOBAL_COORDINATORS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_GLOBAL_COORDINATORS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_GLOBAL_COORDINATORS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_EDITION_GLOBAL_COORDINATOR.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_GLOBAL_COORDINATOR.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_GLOBAL_COORDINATOR.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_EDITION_GLOBAL_COORDINATOR.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_GLOBAL_COORDINATOR.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_GLOBAL_COORDINATOR.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.CREATE_GLOBAL_COORDINATOR.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.EDIT_GLOBAL_COORDINATOR.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_GLOBAL_COORDINATOR.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.EDIT_GLOBAL_COORDINATOR.FAILURE:
      return { ...state, formSaving: false };

    case actions.REMOVE_GLOBAL_COORDINATOR.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_GLOBAL_COORDINATOR.SUCCESS:
    case actions.REMOVE_GLOBAL_COORDINATOR.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    default:
      return state;
  }
}
