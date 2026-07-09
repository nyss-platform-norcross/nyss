import { push } from "connected-react-router";
import {
  OPEN_PROJECT_ORGANIZATIONS_LIST,
  GET_PROJECT_ORGANIZATIONS,
  OPEN_PROJECT_ORGANIZATION_CREATION,
  CREATE_PROJECT_ORGANIZATION,
  REMOVE_PROJECT_ORGANIZATION,
} from "./projectOrganizationsConstants";

export const goToList = (projectId) =>
  push(`/projects/${projectId}/organizations`);
export const goToCreation = (projectId) =>
  push(`/projects/${projectId}/organizations/add`);

export const openList = {
  invoke: (projectId) => ({
    type: OPEN_PROJECT_ORGANIZATIONS_LIST.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_ORGANIZATIONS_LIST.REQUEST }),
  success: (projectId) => ({
    type: OPEN_PROJECT_ORGANIZATIONS_LIST.SUCCESS,
    projectId,
  }),
  failure: (message) => ({
    type: OPEN_PROJECT_ORGANIZATIONS_LIST.FAILURE,
    message,
  }),
};

export const getList = {
  invoke: (projectId) => ({
    type: GET_PROJECT_ORGANIZATIONS.INVOKE,
    projectId,
  }),
  request: () => ({ type: GET_PROJECT_ORGANIZATIONS.REQUEST }),
  success: (list) => ({ type: GET_PROJECT_ORGANIZATIONS.SUCCESS, list }),
  failure: (message) => ({ type: GET_PROJECT_ORGANIZATIONS.FAILURE, message }),
};

export const openCreation = {
  invoke: (projectId) => ({
    type: OPEN_PROJECT_ORGANIZATION_CREATION.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_ORGANIZATION_CREATION.REQUEST }),
  success: (formData) => ({
    type: OPEN_PROJECT_ORGANIZATION_CREATION.SUCCESS,
    formData,
  }),
  failure: (message) => ({
    type: OPEN_PROJECT_ORGANIZATION_CREATION.FAILURE,
    message,
  }),
};

export const create = {
  invoke: (projectId, data) => ({
    type: CREATE_PROJECT_ORGANIZATION.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: CREATE_PROJECT_ORGANIZATION.REQUEST }),
  success: () => ({ type: CREATE_PROJECT_ORGANIZATION.SUCCESS }),
  failure: (error) => ({
    type: CREATE_PROJECT_ORGANIZATION.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const remove = {
  invoke: (projectId, projectOrganizationId) => ({
    type: REMOVE_PROJECT_ORGANIZATION.INVOKE,
    projectId,
    projectOrganizationId,
  }),
  request: (id) => ({ type: REMOVE_PROJECT_ORGANIZATION.REQUEST, id }),
  success: (id) => ({ type: REMOVE_PROJECT_ORGANIZATION.SUCCESS, id }),
  failure: (id, message) => ({
    type: REMOVE_PROJECT_ORGANIZATION.FAILURE,
    id,
    message,
  }),
};
