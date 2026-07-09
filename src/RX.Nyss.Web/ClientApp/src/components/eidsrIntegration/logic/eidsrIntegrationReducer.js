import * as actions from "./eidsrIntegrationConstants";
import * as nationalSocietyActions from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";
import { GET_EIDSR_ORGANISATION_UNITS } from "./eidsrIntegrationConstants";

export function eidsrIntegrationReducer(
  state = initialState.eidsrIntegration,
  action,
) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state, data: null, formError: null };

    case actions.GET_EIDSR_INTEGRATION.INVOKE:
      return { ...state, isFetching: true };

    case actions.GET_EIDSR_INTEGRATION.REQUEST:
      return { ...state };

    case actions.GET_EIDSR_INTEGRATION.SUCCESS:
      return { ...state, data: action.data, isFetching: false };

    case actions.GET_EIDSR_INTEGRATION.FAILURE:
      return { ...state, isFetching: false };

    case actions.EDIT_EIDSR_INTEGRATION.REQUEST:
      return { ...state, formSaving: true };

    case actions.EDIT_EIDSR_INTEGRATION.SUCCESS:
      return { ...state, formSaving: false, data: {} };

    case actions.EDIT_EIDSR_INTEGRATION.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.GET_EIDSR_ORGANISATION_UNITS.REQUEST:
      return { ...state, organisationUnitsIsFetching: true };

    case actions.GET_EIDSR_ORGANISATION_UNITS.SUCCESS:
      return {
        ...state,
        organisationUnits: action.organisationUnits,
        organisationUnitsIsFetching: false,
      };

    case actions.GET_EIDSR_ORGANISATION_UNITS.FAILURE:
      return {
        ...state,
        organisationUnitsIsFetching: false,
        formError: action.error,
      };

    case actions.GET_EIDSR_PROGRAM.REQUEST:
      return { ...state, programIsFetching: true };

    case actions.GET_EIDSR_PROGRAM.SUCCESS:
      return { ...state, program: action.program, programIsFetching: false };

    case actions.GET_EIDSR_PROGRAM.FAILURE:
      return { ...state, programIsFetching: false, formError: action.error };

    default:
      return state;
  }
}
