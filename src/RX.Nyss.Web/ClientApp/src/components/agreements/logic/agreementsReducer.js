import * as actions from "./agreementsConstants";
import { initialState } from "../../../initialState";

export function agreementsReducer(state = initialState.agreements, action) {
  switch (action.type) {
    case actions.OPEN_AGREEMENT_PAGE.REQUEST:
      return {
        ...state,
        pendingSocieties: [],
        staleSocieties: [],
        agreementDocuments: [],
      };

    case actions.OPEN_AGREEMENT_PAGE.SUCCESS:
      return {
        ...state,
        pendingSocieties: action.pendingAgreementDocuments.pendingSocieties,
        staleSocieties: action.pendingAgreementDocuments.staleSocieties,
        agreementDocuments: action.pendingAgreementDocuments.agreementDocuments,
      };

    case actions.OPEN_AGREEMENT_PAGE.FAILURE:
      return {
        ...state,
        pendingSocieties: [],
        staleSocieties: [],
        agreementDocuments: [],
      };

    case actions.ACCEPT_AGREEMENT.REQUEST:
      return { ...state, submitting: true };

    case actions.ACCEPT_AGREEMENT.SUCCESS:
      return { ...state, submitting: false };

    case actions.ACCEPT_AGREEMENT.FAILURE:
      return { ...state, submitting: false };

    default:
      return state;
  }
}
