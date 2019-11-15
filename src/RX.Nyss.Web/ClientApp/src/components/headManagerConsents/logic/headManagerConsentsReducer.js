import * as actions from "./headManagerConsentsConstants";
import { initialState } from "../../../initialState";

export function headManagerConsentsReducer(state = initialState.headManagerConsents, action) {
  switch (action.type) {

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.REQUEST:
      return { ...state, fetching: true, nationalSocieties: [] };

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.SUCCESS:
      return { ...state, fetching: false, nationalSocieties: action.list };

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.FAILURE:
      return { ...state, fetching: false };

    case actions.CONSENT_AS_HEAD_MANAGER.REQUEST:
      return { ...state, submitting: true };

    case actions.CONSENT_AS_HEAD_MANAGER.SUCCESS:
      return { ...state, submitting: false };

    case actions.CONSENT_AS_HEAD_MANAGER.FAILURE:
      return { ...state, submitting: false, consentAsHeadManagerErrorMessage: action.message };

    default:
      return state;
  }
};
