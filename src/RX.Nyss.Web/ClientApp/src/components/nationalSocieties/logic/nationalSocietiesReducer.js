import * as actions from "./nationalSocietiesConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function nationalSocietiesReducer(state = initialState.nationalSocieties, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null }

    case actions.GET_NATIONAL_SOCIETIES.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_NATIONAL_SOCIETIES.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_NATIONAL_SOCIETIES.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_NATIONAL_SOCIETY_OVERVIEW.REQUEST:
      return { ...state, overviewFetching: true, overviewData: null };

    case actions.OPEN_NATIONAL_SOCIETY_OVERVIEW.SUCCESS:
      return { ...state, overviewFetching: false, overviewData: action.data };

    case actions.OPEN_NATIONAL_SOCIETY_OVERVIEW.FAILURE:
      return { ...state, overviewFetching: false };

    case actions.CREATE_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.CREATE_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.EDIT_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.EDIT_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.REMOVE_NATIONAL_SOCIETY.REQUEST:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, true) };

    case actions.REMOVE_NATIONAL_SOCIETY.SUCCESS:
    case actions.REMOVE_NATIONAL_SOCIETY.FAILURE:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined) };

    default:
      return state;
  }
};
