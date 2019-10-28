import * as actions from "./appConstans";

export const initApplication = {
  invoke: () =>
    ({ type: actions.INIT_APPLICATION.INVOKE }),
  request: () =>

    ({ type: actions.INIT_APPLICATION.REQUEST }),

  success: () =>
    ({ type: actions.INIT_APPLICATION.SUCCESS }),

  failure: (message) =>
    ({ type: actions.INIT_APPLICATION.FAILURE, message })
};

export const getUser = {
  invoke: () =>
    ({ type: actions.GET_USER.INVOKE }),

  request: () =>
    ({ type: actions.GET_USER.REQUEST }),

  success: (isAuthenticated, { name, email, roles }, session) =>
    ({ type: actions.GET_USER.SUCCESS, isAuthenticated, user: { name, email, roles }, session }),

  failure: (message) => ({ type: actions.GET_USER.FAILURE, message })
};

export const getAppData = {
  invoke: () =>
    ({ type: actions.GET_APP_DATA.INVOKE }),

  request: () =>
    ({ type: actions.GET_APP_DATA.REQUEST }),

  success: () =>
    ({ type: actions.GET_APP_DATA.SUCCESS }),

  failure: (message) =>
    ({ type: actions.GET_APP_DATA.FAILURE, message })
};

export const getStrings = {
  invoke: () =>
    ({ type: actions.GET_STRINGS.INVOKE }),

  request: () =>
    ({ type: actions.GET_STRINGS.REQUEST }),

  success: () =>
    ({ type: actions.GET_STRINGS.SUCCESS }),

  failure: (message) =>
    ({ type: actions.GET_STRINGS.FAILURE, message })
};

export const openModule = {
  invoke: (path, params) =>
    ({ type: actions.OPEN_MODULE.INVOKE, path, params }),

  success: (path, parameters, breadcrumb, topMenu, sideMenu) =>
    ({ type: actions.OPEN_MODULE.SUCCESS, path, parameters, breadcrumb, topMenu, sideMenu })
};