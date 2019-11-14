import * as actions from "./nationalSocietyUsersConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function nationalSocietyUsersReducer(state = initialState.nationalSocietyUsers, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null }

    case actions.GET_NATIONAL_SOCIETY_USERS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_NATIONAL_SOCIETY_USERS.SUCCESS:
      return { ...state, listFetching: false, listData: action.list, listStale: false, listNationalSocietyId: action.nationalSocietyId };

    case actions.GET_NATIONAL_SOCIETY_USERS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_NATIONAL_SOCIETY_USER_EDITION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_NATIONAL_SOCIETY_USER_EDITION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_NATIONAL_SOCIETY_USER_EDITION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_NATIONAL_SOCIETY_USER_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_NATIONAL_SOCIETY_USER.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_NATIONAL_SOCIETY_USER.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_NATIONAL_SOCIETY_USER.FAILURE:
      return { ...state, formSaving: false, formError: action.message };

    case actions.EDIT_NATIONAL_SOCIETY_USER.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_NATIONAL_SOCIETY_USER.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_NATIONAL_SOCIETY_USER.FAILURE:
      return { ...state, formSaving: false };

    case actions.REMOVE_NATIONAL_SOCIETY_USER.REQUEST:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, true) };

    case actions.REMOVE_NATIONAL_SOCIETY_USER.SUCCESS:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined), listData: state.listData.filter(d => d.id !== action.id) };

    case actions.REMOVE_NATIONAL_SOCIETY_USER.FAILURE:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined) };

    case actions.SET_AS_HEADMANAGER_NATIONAL_SOCIETY_USER.REQUEST:
      return { ...state, settingAsHead: setProperty(state.settingAsHead, action.id, true) };

    case actions.SET_AS_HEADMANAGER_NATIONAL_SOCIETY_USER.SUCCESS:
      return { ...state, listStale: true, settingAsHead: setProperty(state.settingAsHead, action.id, undefined) };

    case actions.SET_AS_HEADMANAGER_NATIONAL_SOCIETY_USER.FAILURE:
      return { ...state, settingAsHead: setProperty(state.settingAsHead, action.id, undefined), message: action.message };

    default:
      return state;
  }
};
