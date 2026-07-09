import {
  OPEN_TRANSLATIONS_LIST,
  GET_TRANSLATIONS,
  OPEN_EMAIL_TRANSLATIONS_LIST,
  GET_EMAIL_TRANSLATIONS,
  OPEN_SMS_TRANSLATIONS_LIST,
  GET_SMS_TRANSLATIONS,
} from "./translationsConstants";

export const openTranslationsList = {
  invoke: () => ({ type: OPEN_TRANSLATIONS_LIST.INVOKE }),
  request: () => ({ type: OPEN_TRANSLATIONS_LIST.REQUEST }),
  success: () => ({ type: OPEN_TRANSLATIONS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_TRANSLATIONS_LIST.FAILURE, message }),
};

export const getTranslationsList = {
  invoke: (needsImprovementOnly) => ({
    type: GET_TRANSLATIONS.INVOKE,
    needsImprovementOnly,
  }),
  request: () => ({ type: GET_TRANSLATIONS.REQUEST }),
  success: (data) => ({ type: GET_TRANSLATIONS.SUCCESS, data }),
  failure: (message) => ({ type: GET_TRANSLATIONS.FAILURE, message }),
};

export const openEmailTranslationsList = {
  invoke: () => ({ type: OPEN_EMAIL_TRANSLATIONS_LIST.INVOKE }),
  request: () => ({ type: OPEN_EMAIL_TRANSLATIONS_LIST.REQUEST }),
  success: () => ({ type: OPEN_EMAIL_TRANSLATIONS_LIST.SUCCESS }),
  failure: (message) => ({
    type: OPEN_EMAIL_TRANSLATIONS_LIST.FAILURE,
    message,
  }),
};

export const getEmailTranslationsList = {
  invoke: (needsImprovementOnly) => ({
    type: GET_EMAIL_TRANSLATIONS.INVOKE,
    needsImprovementOnly,
  }),
  request: () => ({ type: GET_EMAIL_TRANSLATIONS.REQUEST }),
  success: (data) => ({ type: GET_EMAIL_TRANSLATIONS.SUCCESS, data }),
  failure: (message) => ({ type: GET_EMAIL_TRANSLATIONS.FAILURE, message }),
};

export const openSmsTranslationsList = {
  invoke: () => ({ type: OPEN_SMS_TRANSLATIONS_LIST.INVOKE }),
  request: () => ({ type: OPEN_SMS_TRANSLATIONS_LIST.REQUEST }),
  success: () => ({ type: OPEN_SMS_TRANSLATIONS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_SMS_TRANSLATIONS_LIST.FAILURE, message }),
};

export const getSmsTranslationsList = {
  invoke: (needsImprovementOnly) => ({
    type: GET_SMS_TRANSLATIONS.INVOKE,
    needsImprovementOnly,
  }),
  request: () => ({ type: GET_SMS_TRANSLATIONS.REQUEST }),
  success: (data) => ({ type: GET_SMS_TRANSLATIONS.SUCCESS, data }),
  failure: (message) => ({ type: GET_SMS_TRANSLATIONS.FAILURE, message }),
};
