import * as actions from "./appConstans";
import { push } from "connected-react-router";

export const showMessage = (messageKey, time) => ({
  type: actions.SHOW_MESSAGE.INVOKE,
  messageKey,
  time,
});
export const closeMessage = () => ({ type: actions.CLOSE_MESSAGE.INVOKE });
export const entityUpdated = (entity) => ({
  type: actions.ENTITY_UPDATED,
  entities: [entity],
});
export const switchStrings = (status) => ({
  type: actions.SWITCH_STRINGS,
  status,
});
export const setAppReady = (status) => ({
  type: actions.SET_APP_READY,
  status,
});
export const toggleSideMenu = (value) => ({
  type: actions.TOGGLE_SIDE_MENU,
  value,
});
export const expandSideMenu = (value) => ({
  type: actions.EXPAND_SIDE_MENU,
  value,
});
export const goToTranslations = () => push(`/translations`);
export const pageFocused = () => ({ type: actions.PAGE_FOCUSED });
export const stringsUpdated = (key, translations) => ({
  type: actions.STRINGS_UPDATED,
  key,
  translations,
});
export const smsStringsUpdated = (key, translations) => ({
  type: actions.SMS_STRINGS_UPDATED,
  key,
  translations,
});
export const emailStringsUpdated = (key, translations) => ({
  type: actions.EMAIL_STRINGS_UPDATED,
  key,
  translations,
});
export const stringsDeleted = (key) => ({ type: actions.STRINGS_DELETED, key });
export const smsStringsDeleted = (key) => ({
  type: actions.SMS_STRINGS_DELETED,
  key,
});
export const emailStringsDeleted = (key) => ({
  type: actions.EMAIL_STRINGS_DELETED,
  key,
});

export const initApplication = {
  invoke: () => ({ type: actions.INIT_APPLICATION.INVOKE }),

  request: () => ({ type: actions.INIT_APPLICATION.REQUEST }),

  success: () => ({ type: actions.INIT_APPLICATION.SUCCESS }),

  failure: (message) => ({ type: actions.INIT_APPLICATION.FAILURE, message }),
};

export const getUser = {
  invoke: () => ({ type: actions.GET_USER.INVOKE }),

  request: () => ({ type: actions.GET_USER.REQUEST }),

  success: (isAuthenticated, user) => ({
    type: actions.GET_USER.SUCCESS,
    isAuthenticated,
    user,
  }),

  failure: (message) => ({ type: actions.GET_USER.FAILURE, message }),
};

export const getAppData = {
  invoke: () => ({ type: actions.GET_APP_DATA.INVOKE }),

  request: () => ({ type: actions.GET_APP_DATA.REQUEST }),

  success: (
    contentLanguages,
    countries,
    isDevelopment,
    isDemo,
    authCookieExpiration,
    applicationInsightsConnectionString,
  ) => ({
    type: actions.GET_APP_DATA.SUCCESS,
    contentLanguages,
    countries,
    isDevelopment,
    isDemo,
    authCookieExpiration,
    applicationInsightsConnectionString,
  }),

  failure: (message) => ({ type: actions.GET_APP_DATA.FAILURE, message }),
};

export const getStrings = {
  invoke: () => ({ type: actions.GET_STRINGS.INVOKE }),

  request: () => ({ type: actions.GET_STRINGS.REQUEST }),

  success: () => ({ type: actions.GET_STRINGS.SUCCESS }),

  failure: (message) => ({ type: actions.GET_STRINGS.FAILURE, message }),
};

export const openModule = {
  invoke: (path, params) => ({
    type: actions.OPEN_MODULE.INVOKE,
    path,
    params,
  }),

  success: (
    path,
    parameters,
    generalMenu,
    sideMenu,
    tabMenu,
    projectTabMenu,
    title,
    subTitle,
  ) => ({
    type: actions.OPEN_MODULE.SUCCESS,
    path,
    parameters,
    generalMenu,
    sideMenu,
    tabMenu,
    projectTabMenu,
    title,
    subTitle,
  }),

  failure: (message) => ({ type: actions.OPEN_MODULE.FAILURE, message }),
};

export const sendFeedback = {
  invoke: (message) => ({ type: actions.SEND_FEEDBACK.INVOKE, message }),
  request: () => ({ type: actions.SEND_FEEDBACK.REQUEST }),
  success: () => ({ type: actions.SEND_FEEDBACK.SUCCESS }),
  failure: (message) => ({ type: actions.SEND_FEEDBACK.FAILURE, message }),
};
