import * as actions from "./authConstants";

export const login = {
  invoke: (userName, password, redirectUrl) => actions.LOGIN.invoke({ userName, password, redirectUrl }),
  request: () => actions.LOGIN.request(),
  success: () => actions.LOGIN.success(),
  failure: (message) => actions.LOGIN.failure({ message })
};

export const logout = {
  invoke: () => actions.LOGOUT.invoke(),
  request: () => actions.LOGOUT.request(),
  success: () => actions.LOGOUT.success(),
  failure: (message) => actions.LOGOUT.failure({ message })
};