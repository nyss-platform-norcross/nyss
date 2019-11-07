import * as actions from "./dataCollectorsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function dataCollectorsReducer(state = initialState.dataCollectors, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null }

    case actions.GET_DATA_COLLECTORS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_DATA_COLLECTORS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list, listStale: false };

    case actions.GET_DATA_COLLECTORS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_DATA_COLLECTOR_EDITION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_DATA_COLLECTOR_EDITION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_DATA_COLLECTOR_EDITION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_DATA_COLLECTOR_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_DATA_COLLECTOR.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_DATA_COLLECTOR.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_DATA_COLLECTOR.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.EDIT_DATA_COLLECTOR.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_DATA_COLLECTOR.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_DATA_COLLECTOR.FAILURE:
      return { ...state, formSaving: false };

    case actions.REMOVE_DATA_COLLECTOR.REQUEST:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, true) };

    case actions.REMOVE_DATA_COLLECTOR.SUCCESS:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined), listStale: true };

    case actions.REMOVE_DATA_COLLECTOR.FAILURE:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined) };

    default:
      return state;
  }
};
