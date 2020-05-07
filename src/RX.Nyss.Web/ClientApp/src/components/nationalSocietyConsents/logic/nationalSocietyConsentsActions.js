import { OPEN_HEAD_MANAGER_CONSENTS_PAGE, CONSENT_AS_HEAD_MANAGER } from "./nationalSocietyConsentsConstants";

export const openNationalSocietyConsentsPage = {
  invoke: () => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.INVOKE }),
  request: () => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.REQUEST }),
  success: (pendingHeadManagerConsent) => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.SUCCESS, pendingHeadManagerConsent }),
  failure: (message) => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.FAILURE, message })
};

export const consentAsHeadManager = {
  invoke: (selectedLanguage) => ({ type: CONSENT_AS_HEAD_MANAGER.INVOKE, selectedLanguage }),
  request: () => ({ type: CONSENT_AS_HEAD_MANAGER.REQUEST }),
  success: () => ({ type: CONSENT_AS_HEAD_MANAGER.SUCCESS }),
  failure: (message) => ({ type: CONSENT_AS_HEAD_MANAGER.FAILURE, message, suppressPopup: true })
};
