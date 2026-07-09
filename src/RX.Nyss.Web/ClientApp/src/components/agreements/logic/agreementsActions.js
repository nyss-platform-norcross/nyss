import { OPEN_AGREEMENT_PAGE, ACCEPT_AGREEMENT } from "./agreementsConstants";

export const openAgreementPage = {
  invoke: () => ({ type: OPEN_AGREEMENT_PAGE.INVOKE }),
  request: () => ({ type: OPEN_AGREEMENT_PAGE.REQUEST }),
  success: (pendingAgreementDocuments) => ({
    type: OPEN_AGREEMENT_PAGE.SUCCESS,
    pendingAgreementDocuments,
  }),
  failure: (message) => ({ type: OPEN_AGREEMENT_PAGE.FAILURE, message }),
};

export const acceptAgreement = {
  invoke: (selectedLanguage) => ({
    type: ACCEPT_AGREEMENT.INVOKE,
    selectedLanguage,
  }),
  request: () => ({ type: ACCEPT_AGREEMENT.REQUEST }),
  success: () => ({ type: ACCEPT_AGREEMENT.SUCCESS }),
  failure: (message) => ({
    type: ACCEPT_AGREEMENT.FAILURE,
    message,
    suppressPopup: true,
  }),
};
