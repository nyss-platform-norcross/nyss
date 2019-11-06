import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_USERS_LIST, GET_NATIONAL_SOCIETY_USERS,
  OPEN_NATIONAL_SOCIETY_USER_CREATION, CREATE_NATIONAL_SOCIETY_USER,
  OPEN_NATIONAL_SOCIETY_USER_EDITION, EDIT_NATIONAL_SOCIETY_USER,
  REMOVE_NATIONAL_SOCIETY_USER
} from "./nationalSocietyUsersConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/users`);
export const goToCreation = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/users/add`);
export const goToEdition = (nationalSocietyId, nationalSocietyUserId) => push(`/nationalsocieties/${nationalSocietyId}/users/${nationalSocietyUserId}/edit`);

export const openList = {
  invoke: (nationalSocietyId) => ({ type: OPEN_NATIONAL_SOCIETY_USERS_LIST.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USERS_LIST.REQUEST }),
  success: () => ({ type: OPEN_NATIONAL_SOCIETY_USERS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_NATIONAL_SOCIETY_USERS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (nationalSocietyId) => ({ type: GET_NATIONAL_SOCIETY_USERS.INVOKE, nationalSocietyId }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_USERS.REQUEST }),
  success: (list) => ({ type: GET_NATIONAL_SOCIETY_USERS.SUCCESS, list }),
  failure: (message) => ({ type: GET_NATIONAL_SOCIETY_USERS.FAILURE, message })
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({ type: OPEN_NATIONAL_SOCIETY_USER_CREATION.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_CREATION.REQUEST }),
  success: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_CREATION.SUCCESS }),
  failure: (message) => ({ type: OPEN_NATIONAL_SOCIETY_USER_CREATION.FAILURE, message })
};

export const create = {
  invoke: (nationalSocietyId, data) => ({ type: CREATE_NATIONAL_SOCIETY_USER.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: CREATE_NATIONAL_SOCIETY_USER.REQUEST }),
  success: () => ({ type: CREATE_NATIONAL_SOCIETY_USER.SUCCESS }),
  failure: (message) => ({ type: CREATE_NATIONAL_SOCIETY_USER.FAILURE, message })
};

export const openEdition = {
  invoke: (nationalSocietyUserId, role) => ({ type: OPEN_NATIONAL_SOCIETY_USER_EDITION.INVOKE, nationalSocietyUserId, role }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_EDITION.REQUEST }),
  success: (data) => ({ type: OPEN_NATIONAL_SOCIETY_USER_EDITION.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_NATIONAL_SOCIETY_USER_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (nationalSocietyId, data) => ({ type: EDIT_NATIONAL_SOCIETY_USER.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: EDIT_NATIONAL_SOCIETY_USER.REQUEST }),
  success: () => ({ type: EDIT_NATIONAL_SOCIETY_USER.SUCCESS }),
  failure: (message) => ({ type: EDIT_NATIONAL_SOCIETY_USER.FAILURE, message })
};

export const remove = {
  invoke: (nationalSocietyUserId, role) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.INVOKE, nationalSocietyUserId, role }),
  request: (id) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.REQUEST, id }),
  success: (id) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.FAILURE, id, message })
};