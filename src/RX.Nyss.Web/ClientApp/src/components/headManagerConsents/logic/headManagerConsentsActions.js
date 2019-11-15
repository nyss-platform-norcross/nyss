import { OPEN_HEAD_MANAGER_CONSENTS_PAGE, CONSENT_AS_HEAD_MANAGER } from "./headManagerConsentsConstants";

export const openHeadManagerConsentsPage = {
  invoke: () => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.INVOKE }),
  request: () => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.REQUEST }),
  success: (list) => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.SUCCESS, list }),
  failure: (message) => ({ type: OPEN_HEAD_MANAGER_CONSENTS_PAGE.FAILURE, message })
};

export const consentAsHeadManager = {
  invoke: () => ({ type: CONSENT_AS_HEAD_MANAGER.INVOKE }),
  request: () => ({ type: CONSENT_AS_HEAD_MANAGER.REQUEST }),
  success: () => ({ type: CONSENT_AS_HEAD_MANAGER.SUCCESS }),
  failure: (message) => ({ type: CONSENT_AS_HEAD_MANAGER.FAILURE, message })
};
