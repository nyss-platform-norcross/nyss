import * as actions from "./nationalSocietiesConstants";

export const getList = {
  invoke: (userName, password, redirectUrl) =>
    actions.GET_NATIONAL_SOCIETIES.invoke({ userName, password, redirectUrl }),

  request: () =>
    actions.GET_NATIONAL_SOCIETIES.request(),

  success: (list) =>
    actions.GET_NATIONAL_SOCIETIES.success({ list }),

  failure: (message) =>
    actions.GET_NATIONAL_SOCIETIES.failure({ message })
};