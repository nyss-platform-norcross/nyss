import { push } from "connected-react-router";
import {
  OPEN_ORGANIZATIONS_LIST,
  GET_ORGANIZATIONS,
  OPEN_ORGANIZATION_CREATION,
  CREATE_ORGANIZATION,
  OPEN_ORGANIZATION_EDITION,
  EDIT_ORGANIZATION,
  REMOVE_ORGANIZATION,
} from "./organizationsConstants";

export const goToList = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/organizations`);
export const goToCreation = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/organizations/add`);
export const goToEdition = (nationalSocietyId, organizationId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/organizations/${organizationId}/edit`,
  );

export const openList = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_ORGANIZATIONS_LIST.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_ORGANIZATIONS_LIST.REQUEST }),
  success: (nationalSocietyId) => ({
    type: OPEN_ORGANIZATIONS_LIST.SUCCESS,
    nationalSocietyId,
  }),
  failure: (error) => ({ type: OPEN_ORGANIZATIONS_LIST.FAILURE, error }),
};

export const getList = {
  invoke: (nationalSocietyId) => ({
    type: GET_ORGANIZATIONS.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: GET_ORGANIZATIONS.REQUEST }),
  success: (list) => ({ type: GET_ORGANIZATIONS.SUCCESS, list }),
  failure: (error) => ({ type: GET_ORGANIZATIONS.FAILURE, error }),
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_ORGANIZATION_CREATION.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_ORGANIZATION_CREATION.REQUEST }),
  success: () => ({ type: OPEN_ORGANIZATION_CREATION.SUCCESS }),
  failure: (error) => ({ type: OPEN_ORGANIZATION_CREATION.FAILURE, error }),
};

export const create = {
  invoke: (nationalSocietyId, data) => ({
    type: CREATE_ORGANIZATION.INVOKE,
    nationalSocietyId,
    data,
  }),
  request: () => ({ type: CREATE_ORGANIZATION.REQUEST }),
  success: () => ({ type: CREATE_ORGANIZATION.SUCCESS }),
  failure: (error) => ({
    type: CREATE_ORGANIZATION.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: (nationalSocietyId, organizationId) => ({
    type: OPEN_ORGANIZATION_EDITION.INVOKE,
    nationalSocietyId,
    organizationId,
  }),
  request: () => ({ type: OPEN_ORGANIZATION_EDITION.REQUEST }),
  success: (data) => ({ type: OPEN_ORGANIZATION_EDITION.SUCCESS, data }),
  failure: (error) => ({ type: OPEN_ORGANIZATION_EDITION.FAILURE, error }),
};

export const edit = {
  invoke: (nationalSocietyId, data) => ({
    type: EDIT_ORGANIZATION.INVOKE,
    nationalSocietyId,
    data,
  }),
  request: () => ({ type: EDIT_ORGANIZATION.REQUEST }),
  success: () => ({ type: EDIT_ORGANIZATION.SUCCESS }),
  failure: (error) => ({
    type: EDIT_ORGANIZATION.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const remove = {
  invoke: (nationalSocietyId, organizationId) => ({
    type: REMOVE_ORGANIZATION.INVOKE,
    nationalSocietyId,
    organizationId,
  }),
  request: (id) => ({ type: REMOVE_ORGANIZATION.REQUEST, id }),
  success: (id) => ({ type: REMOVE_ORGANIZATION.SUCCESS, id }),
  failure: (id, error) => ({ type: REMOVE_ORGANIZATION.FAILURE, id, error }),
};
