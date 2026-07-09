import * as actions from "./projectsConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function projectsReducer(state = initialState.projects, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null };

    case actions.GET_PROJECTS.REQUEST:
      return { ...state, listFetching: true, listData: [] };

    case actions.GET_PROJECTS.SUCCESS:
      return {
        ...state,
        listFetching: false,
        listData: action.list,
        listStale: true,
      };

    case actions.GET_PROJECTS.FAILURE:
      return { ...state, listFetching: false, listData: [] };

    case actions.OPEN_PROJECT_CREATION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_PROJECT_CREATION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_PROJECT_CREATION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data };

    case actions.OPEN_PROJECT_CREATION.FAILURE:
      return { ...state, formFetching: false, formError: action.error };

    case actions.OPEN_PROJECT_EDITION.INVOKE:
      return {
        ...state,
        formFetching: true,
        formData: null,
        formHealthRisks: [],
        formTimeZones: [],
      };

    case actions.OPEN_PROJECT_EDITION.REQUEST:
      return {
        ...state,
        formFetching: true,
        formData: null,
        formHealthRisks: [],
        formTimeZones: [],
      };

    case actions.OPEN_PROJECT_EDITION.SUCCESS:
      return {
        ...state,
        formFetching: false,
        formData: action.data,
        formHealthRisks: action.healthRisks,
        formTimeZones: action.timeZones,
      };

    case actions.OPEN_PROJECT_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_PROJECT_HEALTHRISKS_EDITION.INVOKE:
      return {
        ...state,
        formFetching: true,
        formData: null,
        formHealthRisks: [],
        formTimeZones: [],
      };

    case actions.OPEN_PROJECT_HEALTHRISKS_EDITION.REQUEST:
      return {
        ...state,
        formFetching: true,
        formData: null,
        formHealthRisks: [],
        formTimeZones: [],
      };

    case actions.OPEN_PROJECT_HEALTHRISKS_EDITION.SUCCESS:
      return {
        ...state,
        formFetching: false,
        formData: action.data,
        formHealthRisks: action.healthRisks,
        formTimeZones: action.timeZones,
      };

    case actions.OPEN_PROJECT_HEALTHRISKS_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_PROJECT_OVERVIEW.INVOKE:
      return {
        ...state,
        formFetching: true,
        overviewData: null,
        overviewHealthRisks: [],
        overviewTimeZones: [],
      };

    case actions.OPEN_PROJECT_OVERVIEW.REQUEST:
      return {
        ...state,
        formFetching: true,
        overviewData: null,
        overviewHealthRisks: [],
        overviewTimeZones: [],
      };

    case actions.OPEN_PROJECT_OVERVIEW.SUCCESS:
      return {
        ...state,
        formFetching: false,
        overviewData: action.data,
        overviewHealthRisks: action.healthRisks,
        overviewTimeZones: action.timeZones,
      };

    case actions.OPEN_PROJECT_OVERVIEW.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.INVOKE:
      return {
        ...state,
        formFetching: true,
        overviewData: null,
        overviewHealthRisks: [],
        overviewTimeZones: [],
      };

    case actions.OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.REQUEST:
      return {
        ...state,
        formFetching: true,
        overviewData: null,
        overviewHealthRisks: [],
        overviewTimeZones: [],
      };

    case actions.OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.SUCCESS:
      return {
        ...state,
        formFetching: false,
        overviewData: action.data,
        overviewHealthRisks: action.healthRisks,
        overviewTimeZones: action.timeZones,
      };

    case actions.OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.FAILURE:
      return { ...state, formFetching: false };

    case actions.CREATE_PROJECT.REQUEST:
      return { ...state, formSaving: true };

    case actions.CREATE_PROJECT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_PROJECT.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_PROJECT.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_PROJECT.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_PROJECT.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_PROJECT_HEALTH_RISKS.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_PROJECT_HEALTH_RISKS.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_PROJECT_HEALTH_RISKS.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.CLOSE_PROJECT.REQUEST:
      return {
        ...state,
        isClosing: setProperty(state.isClosing, action.id, true),
      };

    case actions.CLOSE_PROJECT.SUCCESS:
      return {
        ...state,
        isClosing: setProperty(state.isClosing, action.id, undefined),
        listStale: true,
      };

    case actions.CLOSE_PROJECT.FAILURE:
      return {
        ...state,
        isClosing: setProperty(state.isClosing, action.id, undefined),
      };

    default:
      return state;
  }
}
