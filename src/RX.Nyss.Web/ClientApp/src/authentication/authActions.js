import {
  LOGIN,
  LOGOUT,
  VERIFY_EMAIL,
  RESET_PASSWORD,
  RESET_PASSWORD_CALLBACK,
} from "./authConstants";

export const login = {
  invoke: (userName, password, redirectUrl) => ({
    type: LOGIN.INVOKE,
    userName,
    password,
    redirectUrl,
  }),

  request: () => ({ type: LOGIN.REQUEST }),

  success: () => ({ type: LOGIN.SUCCESS }),

  failure: (message) => ({ type: LOGIN.FAILURE, message, suppressPopup: true }),
};

export const logout = {
  invoke: () => ({ type: LOGOUT.INVOKE }),

  request: () => ({ type: LOGOUT.REQUEST }),

  success: () => ({ type: LOGOUT.SUCCESS }),

  failure: (message) => ({ type: LOGOUT.FAILURE, message }),
};

export const verifyEmail = {
  invoke: (password, email, token) => ({
    type: VERIFY_EMAIL.INVOKE,
    password,
    email,
    token,
  }),

  request: () => ({ type: VERIFY_EMAIL.REQUEST }),

  success: () => ({ type: VERIFY_EMAIL.SUCCESS }),

  failure: (message) => ({ type: VERIFY_EMAIL.FAILURE, message }),
};

export const resetPassword = {
  invoke: (email) => ({ type: RESET_PASSWORD.INVOKE, email }),

  request: () => ({ type: RESET_PASSWORD.REQUEST }),

  success: () => ({ type: RESET_PASSWORD.SUCCESS }),

  failure: (message) => ({
    type: RESET_PASSWORD.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const resetPasswordCallback = {
  invoke: (password, email, token) => ({
    type: RESET_PASSWORD_CALLBACK.INVOKE,
    password,
    email,
    token,
  }),

  request: () => ({ type: RESET_PASSWORD_CALLBACK.REQUEST }),

  success: () => ({ type: RESET_PASSWORD_CALLBACK.SUCCESS }),

  failure: (message) => ({ type: RESET_PASSWORD_CALLBACK.FAILURE, message }),
};
