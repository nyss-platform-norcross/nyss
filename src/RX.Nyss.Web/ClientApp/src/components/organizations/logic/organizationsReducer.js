import * as actions from "./organizationsConstants";
import * as nationalSocietyActions from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function organizationsReducer(
  state = initialState.organizations,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.OPEN_ORGANIZATIONS_LIST.INVOKE:
      return {
        ...state,
        listStale:
          state.listStale ||
          action.nationalSocietyId !== state.listNationalSocietyId,
      };

    case actions.OPEN_ORGANIZATIONS_LIST.SUCCESS:
      return { ...state, listNationalSocietyId: action.nationalSocietyId };

    case actions.GET_ORGANIZATIONS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_ORGANIZATIONS.SUCCESS:
      return {
        ...state,
        listFetching: false,
        listData: action.list,
        listStale: false,
      };

    case actions.GET_ORGANIZATIONS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_ORGANIZATION_EDITION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_ORGANIZATION_EDITION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_ORGANIZATION_EDITION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_ORGANIZATION_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_ORGANIZATION.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_ORGANIZATION.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_ORGANIZATION.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_ORGANIZATION.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_ORGANIZATION.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_ORGANIZATION.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.REMOVE_ORGANIZATION.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_ORGANIZATION.SUCCESS:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
        listStale: true,
      };

    case actions.REMOVE_ORGANIZATION.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    case nationalSocietyActions.ARCHIVE_NATIONAL_SOCIETY.SUCCESS:
      return { ...state, listStale: true };

    default:
      return state;
  }
}
