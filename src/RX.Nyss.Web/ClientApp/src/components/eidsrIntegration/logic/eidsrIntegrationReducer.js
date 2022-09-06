import * as actions from "./eidsrIntegrationConstants";
import * as nationalSocietyActions from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { initialState } from "../../../initialState";
import { setProperty } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

export function eidsrIntegrationReducer(state = initialState.eidsrIntegration, action) {
  switch (action.type) {
    case LOCATION_CHANGE: // cleanup
      return { ...state }


    case actions.GET_EIDSR_INTEGRATION.INVOKE:
      return { ...state, isFetching: true };

    case actions.GET_EIDSR_INTEGRATION.REQUEST:
      return { ...state };

    case actions.GET_EIDSR_INTEGRATION.SUCCESS:
      return { ...state, data: action.data, isFetching: false };

    case actions.GET_EIDSR_INTEGRATION.FAILURE:
      return { ...state, isFetching: false };


    case actions.EDIT_EIDSR_INTEGRATION.REQUEST:
      return { ...state };

    case actions.EDIT_EIDSR_INTEGRATION.SUCCESS:
      return { ...state };

    case actions.EDIT_EIDSR_INTEGRATION.FAILURE:
      return { ...state };


    default:
      return state;
  }
};
