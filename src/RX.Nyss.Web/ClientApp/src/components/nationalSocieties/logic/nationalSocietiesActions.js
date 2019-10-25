import * as actions from "./nationalSocietiesConstants";
import { push } from "connected-react-router";

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

export const add = {
  invoke: () =>
    push("/nationalsocieties/add"),
};

export const showList = {
  invoke: () =>
    push("/nationalsocieties"),
};

export const create = {
  invoke: (data) =>
    actions.CREATE_NATIONAL_SOCIETY.invoke(data),

  request: () =>
    actions.CREATE_NATIONAL_SOCIETY.request(),

  success: () =>
    actions.CREATE_NATIONAL_SOCIETY.success(),

  failure: (message) =>
    actions.CREATE_NATIONAL_SOCIETY.failure({ message })
};

export const remove = {
  invoke: (id) =>
    actions.REMOVE_NATIONAL_SOCIETY.invoke({ id }),

  request: (id) =>
    actions.REMOVE_NATIONAL_SOCIETY.request({ id }),

  success: (id) =>
    actions.REMOVE_NATIONAL_SOCIETY.success({ id }),

  failure: (id, message) =>
    actions.REMOVE_NATIONAL_SOCIETY.failure({ id, message })
};