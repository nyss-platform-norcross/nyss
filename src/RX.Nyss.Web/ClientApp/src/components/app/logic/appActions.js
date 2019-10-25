import * as actions from "./appConstans";

export const initApplication = {
    invoke: () => actions.INIT_APPLICATION.invoke(),
    request: () => actions.INIT_APPLICATION.request(),
    success: () => actions.INIT_APPLICATION.success(),
    failure: (message) => actions.INIT_APPLICATION.failure({ message })
};

export const getUser = {
    invoke: () => actions.GET_USER.invoke(),
    request: () => actions.GET_USER.request(),
    success: (isAuthenticated, { name, email, roles }, session) =>
        actions.GET_USER.success({ isAuthenticated, user: { name, email, roles }, session }),
    failure: (message) => actions.GET_USER.failure({ message })
};

export const getAppData = {
    invoke: () => actions.GET_APP_DATA.invoke(),
    request: () => actions.GET_APP_DATA.request(),
    success: () => actions.GET_APP_DATA.success(),
    failure: (message) => actions.GET_APP_DATA.failure({ message })
};

export const getStrings = {
    invoke: () => actions.GET_STRINGS.invoke(),
    request: () => actions.GET_STRINGS.request(),
    success: () => actions.GET_STRINGS.success(),
    failure: (message) => actions.GET_STRINGS.failure({ message })
};

export const updateSiteMap = (path, parameters) => actions.UPDATE_SITEMAP.invoke({ path, parameters });