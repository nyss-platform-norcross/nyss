import * as actions from "./nationalSocietyConsentsConstants";
import { initialState } from "../../../initialState";

export function nationalSocietyConsentsReducer(state = initialState.nationalSocietyConsents, action) {
  switch (action.type) {

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.REQUEST:
      return { ...state, fetching: true, nationalSocieties: [], agreementDocuments: [] };

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.SUCCESS:
      return { ...state, fetching: false, pendingSocieties: action.pendingHeadManagerConsent.pendingSocieties, staleSocieties: action.pendingHeadManagerConsent.staleSocieties,agreementDocuments: action.pendingHeadManagerConsent.agreementDocuments };

    case actions.OPEN_HEAD_MANAGER_CONSENTS_PAGE.FAILURE:
      return { ...state, fetching: false };

    case actions.CONSENT_AS_HEAD_MANAGER.REQUEST:
      return { ...state, submitting: true };

    case actions.CONSENT_AS_HEAD_MANAGER.SUCCESS:
      return { ...state, submitting: false };

    case actions.CONSENT_AS_HEAD_MANAGER.FAILURE:
      return { ...state, submitting: false };

    default:
      return state;
  }
};
