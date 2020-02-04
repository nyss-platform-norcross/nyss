import {
  OPEN_TRANSLATIONS_LIST, GET_TRANSLATIONS
} from "./translationsConstants";


export const openList = {
  invoke: () => ({ type: OPEN_TRANSLATIONS_LIST.INVOKE }),
  request: () => ({ type: OPEN_TRANSLATIONS_LIST.REQUEST }),
  success: () => ({ type: OPEN_TRANSLATIONS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_TRANSLATIONS_LIST.FAILURE, message })
};

export const getList = {
  invoke: () => ({ type: GET_TRANSLATIONS.INVOKE }),
  request: () => ({ type: GET_TRANSLATIONS.REQUEST }),
  success: (data) => ({ type: GET_TRANSLATIONS.SUCCESS, data }),
  failure: (message) => ({ type: GET_TRANSLATIONS.FAILURE, message })
};
