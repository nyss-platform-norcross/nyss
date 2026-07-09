import * as actions from "./projectOrganizationsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function projectOrganizationsReducer(
  state = initialState.projectOrganizations,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.OPEN_PROJECT_ORGANIZATIONS_LIST.INVOKE:
      return {
        ...state,
        listStale: state.listStale || action.projectId !== state.listProjectId,
      };

    case actions.OPEN_PROJECT_ORGANIZATIONS_LIST.SUCCESS:
      return { ...state, listProjectId: action.projectId };

    case actions.GET_PROJECT_ORGANIZATIONS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_PROJECT_ORGANIZATIONS.SUCCESS:
      return {
        ...state,
        listFetching: false,
        listData: action.list,
        listStale: false,
      };

    case actions.GET_PROJECT_ORGANIZATIONS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_PROJECT_ORGANIZATION_CREATION.SUCCESS:
      return { ...state, formData: action.formData };

    case actions.CREATE_PROJECT_ORGANIZATION.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_PROJECT_ORGANIZATION.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_PROJECT_ORGANIZATION.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.REMOVE_PROJECT_ORGANIZATION.REQUEST:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, true),
      };

    case actions.REMOVE_PROJECT_ORGANIZATION.SUCCESS:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
        listStale: true,
      };

    case actions.REMOVE_PROJECT_ORGANIZATION.FAILURE:
      return {
        ...state,
        listRemoving: setProperty(state.listRemoving, action.id, undefined),
      };

    default:
      return state;
  }
}
