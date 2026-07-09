import { push } from "connected-react-router";
import {
  OPEN_NATIONAL_SOCIETY_USERS_LIST,
  GET_NATIONAL_SOCIETY_USERS,
  OPEN_NATIONAL_SOCIETY_USER_CREATION,
  OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING,
  CREATE_NATIONAL_SOCIETY_USER,
  ADD_EXISTING_NATIONAL_SOCIETY_USER,
  OPEN_NATIONAL_SOCIETY_USER_EDITION,
  EDIT_NATIONAL_SOCIETY_USER,
  REMOVE_NATIONAL_SOCIETY_USER,
  SET_AS_HEAD_MANAGER,
} from "./nationalSocietyUsersConstants";

export const goToList = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/users`);
export const goToCreation = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/users/add`);
export const goToAddExisting = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/users/addExisting`);
export const goToEdition = (nationalSocietyId, nationalSocietyUserId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/users/${nationalSocietyUserId}/edit`,
  );

export const openList = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_USERS_LIST.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USERS_LIST.REQUEST }),
  success: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_USERS_LIST.SUCCESS,
    nationalSocietyId,
  }),
  failure: (error) => ({
    type: OPEN_NATIONAL_SOCIETY_USERS_LIST.FAILURE,
    error,
  }),
};

export const getList = {
  invoke: (nationalSocietyId) => ({
    type: GET_NATIONAL_SOCIETY_USERS.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: GET_NATIONAL_SOCIETY_USERS.REQUEST }),
  success: (nationalSocietyId, list) => ({
    type: GET_NATIONAL_SOCIETY_USERS.SUCCESS,
    nationalSocietyId,
    list,
  }),
  failure: (error) => ({ type: GET_NATIONAL_SOCIETY_USERS.FAILURE, error }),
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_CREATION.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_CREATION.REQUEST }),
  success: (data) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_CREATION.SUCCESS,
    data,
  }),
  failure: (error) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_CREATION.FAILURE,
    error,
  }),
};

export const openAddExisting = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING.REQUEST }),
  success: (formData) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING.SUCCESS,
    formData,
  }),
  failure: (error) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_ADD_EXISTING.FAILURE,
    error,
  }),
};

export const create = {
  invoke: (nationalSocietyId, data) => ({
    type: CREATE_NATIONAL_SOCIETY_USER.INVOKE,
    nationalSocietyId,
    data,
  }),
  request: () => ({ type: CREATE_NATIONAL_SOCIETY_USER.REQUEST }),
  success: () => ({ type: CREATE_NATIONAL_SOCIETY_USER.SUCCESS }),
  failure: (error) => ({
    type: CREATE_NATIONAL_SOCIETY_USER.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const addExisting = {
  invoke: (data) => ({ type: ADD_EXISTING_NATIONAL_SOCIETY_USER.INVOKE, data }),
  request: () => ({ type: ADD_EXISTING_NATIONAL_SOCIETY_USER.REQUEST }),
  success: () => ({ type: ADD_EXISTING_NATIONAL_SOCIETY_USER.SUCCESS }),
  failure: (error) => ({
    type: ADD_EXISTING_NATIONAL_SOCIETY_USER.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: (nationalSocietyUserId, role) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_EDITION.INVOKE,
    nationalSocietyUserId,
    role,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_USER_EDITION.REQUEST }),
  success: (data, organizations, modems, countryCode) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_EDITION.SUCCESS,
    data,
    organizations,
    modems,
    countryCode,
  }),
  failure: (error) => ({
    type: OPEN_NATIONAL_SOCIETY_USER_EDITION.FAILURE,
    error,
  }),
};

export const edit = {
  invoke: (nationalSocietyId, data) => ({
    type: EDIT_NATIONAL_SOCIETY_USER.INVOKE,
    nationalSocietyId,
    data,
  }),
  request: () => ({ type: EDIT_NATIONAL_SOCIETY_USER.REQUEST }),
  success: () => ({ type: EDIT_NATIONAL_SOCIETY_USER.SUCCESS }),
  failure: (error) => ({
    type: EDIT_NATIONAL_SOCIETY_USER.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const remove = {
  invoke: (nationalSocietyUserId, role, nationalSocietyId) => ({
    type: REMOVE_NATIONAL_SOCIETY_USER.INVOKE,
    nationalSocietyUserId,
    role,
    nationalSocietyId,
  }),
  request: (id) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.REQUEST, id }),
  success: (id) => ({ type: REMOVE_NATIONAL_SOCIETY_USER.SUCCESS, id }),
  failure: (id, error) => ({
    type: REMOVE_NATIONAL_SOCIETY_USER.FAILURE,
    id,
    error,
  }),
};

export const setAsHeadManager = {
  invoke: (organizationId, nationalSocietyUserId) => ({
    type: SET_AS_HEAD_MANAGER.INVOKE,
    organizationId,
    nationalSocietyUserId,
  }),
  request: (id) => ({ type: SET_AS_HEAD_MANAGER.REQUEST, id }),
  success: (id) => ({ type: SET_AS_HEAD_MANAGER.SUCCESS, id }),
  failure: (id, error) => ({ type: SET_AS_HEAD_MANAGER.FAILURE, id, error }),
};
