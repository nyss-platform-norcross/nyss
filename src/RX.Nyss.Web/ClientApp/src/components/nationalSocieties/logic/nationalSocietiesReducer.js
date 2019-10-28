import * as actions from "./nationalSocietiesConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";

export function nationalSocietiesReducer(state = initialState.nationalSocieties, action) {
  switch (action.type) {
    case actions.GET_NATIONAL_SOCIETIES.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_NATIONAL_SOCIETIES.SUCCESS:
      return { ...state, listFetching: false, listData: action.list };

    case actions.GET_NATIONAL_SOCIETIES.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_EDITION_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.CREATE_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formSaving: false };

    case actions.EDIT_NATIONAL_SOCIETY.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, formSaving: false, listData: [] };

    case actions.EDIT_NATIONAL_SOCIETY.FAILURE:
      return { ...state, formSaving: false };

    case actions.REMOVE_NATIONAL_SOCIETY.REQUEST:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, true) };

    case actions.REMOVE_NATIONAL_SOCIETY.SUCCESS:
    case actions.REMOVE_NATIONAL_SOCIETY.FAILURE:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined) };

    default:
      return state;
  }
};
