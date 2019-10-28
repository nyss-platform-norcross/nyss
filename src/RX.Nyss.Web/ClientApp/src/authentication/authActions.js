import { LOGIN, LOGOUT } from "./authConstants";

export const login = {
  invoke: (userName, password, redirectUrl) =>
    ({ type: LOGIN.INVOKE, userName, password, redirectUrl }),

  request: () =>
    ({ type: LOGIN.REQUEST }),

  success: () =>
    ({ type: LOGIN.SUCCESS }),

  failure: (message) =>
    ({ type: LOGIN.FAILURE, message })
};

export const logout = {
  invoke: () =>
    ({ type: LOGOUT.INVOKE }),

  request: () =>
    ({ type: LOGOUT.REQUEST }),

  success: () =>
    ({ type: LOGOUT.SUCCESS }),

  failure: (message) =>
    ({ type: LOGOUT.FAILURE, message })
};