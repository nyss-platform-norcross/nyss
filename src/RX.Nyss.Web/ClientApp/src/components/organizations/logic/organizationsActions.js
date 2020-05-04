import { push } from "connected-react-router";
import {
  OPEN_ORGANIZATIONS_LIST, GET_ORGANIZATIONS,
  OPEN_ORGANIZATION_CREATION, CREATE_ORGANIZATION,
  OPEN_ORGANIZATION_EDITION, EDIT_ORGANIZATION,
  REMOVE_ORGANIZATION
} from "./organizationsConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways`);
export const goToCreation = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/add`);
export const goToEdition = (nationalSocietyId, organizationId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/${organizationId}/edit`);

export const openList = {
  invoke: (nationalSocietyId) => ({ type: OPEN_ORGANIZATIONS_LIST.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_ORGANIZATIONS_LIST.REQUEST }),
  success: (nationalSocietyId) => ({ type: OPEN_ORGANIZATIONS_LIST.SUCCESS, nationalSocietyId }),
  failure: (message) => ({ type: OPEN_ORGANIZATIONS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (nationalSocietyId) => ({ type: GET_ORGANIZATIONS.INVOKE, nationalSocietyId }),
  request: () => ({ type: GET_ORGANIZATIONS.REQUEST }),
  success: (list) => ({ type: GET_ORGANIZATIONS.SUCCESS, list }),
  failure: (message) => ({ type: GET_ORGANIZATIONS.FAILURE, message })
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({ type: OPEN_ORGANIZATION_CREATION.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_ORGANIZATION_CREATION.REQUEST }),
  success: () => ({ type: OPEN_ORGANIZATION_CREATION.SUCCESS }),
  failure: (message) => ({ type: OPEN_ORGANIZATION_CREATION.FAILURE, message })
};

export const create = {
  invoke: (nationalSocietyId, data) => ({ type: CREATE_ORGANIZATION.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: CREATE_ORGANIZATION.REQUEST }),
  success: () => ({ type: CREATE_ORGANIZATION.SUCCESS }),
  failure: (message) => ({ type: CREATE_ORGANIZATION.FAILURE, message, suppressPopup: true })
};

export const openEdition = {
  invoke: (nationalSocietyId, organizationId) => ({ type: OPEN_ORGANIZATION_EDITION.INVOKE, nationalSocietyId, organizationId }),
  request: () => ({ type: OPEN_ORGANIZATION_EDITION.REQUEST }),
  success: (data) => ({ type: OPEN_ORGANIZATION_EDITION.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_ORGANIZATION_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (nationalSocietyId, data) => ({ type: EDIT_ORGANIZATION.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: EDIT_ORGANIZATION.REQUEST }),
  success: () => ({ type: EDIT_ORGANIZATION.SUCCESS }),
  failure: (message) => ({ type: EDIT_ORGANIZATION.FAILURE, message, suppressPopup: true })
};

export const remove = {
  invoke: (nationalSocietyId, organizationId) => ({ type: REMOVE_ORGANIZATION.INVOKE, nationalSocietyId, organizationId }),
  request: (id) => ({ type: REMOVE_ORGANIZATION.REQUEST, id }),
  success: (id) => ({ type: REMOVE_ORGANIZATION.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_ORGANIZATION.FAILURE, id, message })
};
