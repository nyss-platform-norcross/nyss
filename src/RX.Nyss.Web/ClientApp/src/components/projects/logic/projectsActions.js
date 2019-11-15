import { push } from "connected-react-router";
import {
  OPEN_PROJECTS_LIST, GET_PROJECTS,
  OPEN_PROJECT_CREATION, CREATE_PROJECT,
  OPEN_PROJECT_EDITION, EDIT_PROJECT,
  REMOVE_PROJECT,
  OPEN_PROJECT_DASHBOARD
} from "./projectsConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/projects`);
export const goToCreation = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/projects/add`);
export const goToEdition = (nationalSocietyId, projectId) => push(`/nationalsocieties/${nationalSocietyId}/projects/${projectId}/edit`);
export const goToDashboard = (nationalSocietyId, projectId) => push(`/nationalsocieties/${nationalSocietyId}/projects/${projectId}/dashboard`);

export const openList = {
  invoke: (nationalSocietyId) => ({ type: OPEN_PROJECTS_LIST.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_PROJECTS_LIST.REQUEST }),
  success: () => ({ type: OPEN_PROJECTS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_PROJECTS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (nationalSocietyId) => ({ type: GET_PROJECTS.INVOKE, nationalSocietyId }),
  request: () => ({ type: GET_PROJECTS.REQUEST }),
  success: (list) => ({ type: GET_PROJECTS.SUCCESS, list }),
  failure: (message) => ({ type: GET_PROJECTS.FAILURE, message })
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({ type: OPEN_PROJECT_CREATION.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_PROJECT_CREATION.REQUEST }),
  success: (healthRisks) => ({ type: OPEN_PROJECT_CREATION.SUCCESS, healthRisks }),
  failure: (message) => ({ type: OPEN_PROJECT_CREATION.FAILURE, message })
};

export const create = {
  invoke: (nationalSocietyId, data) => ({ type: CREATE_PROJECT.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: CREATE_PROJECT.REQUEST }),
  success: () => ({ type: CREATE_PROJECT.SUCCESS }),
  failure: (message) => ({ type: CREATE_PROJECT.FAILURE, message })
};

export const openEdition = {
  invoke: (nationalSocietyId, projectId) => ({ type: OPEN_PROJECT_EDITION.INVOKE, nationalSocietyId, projectId }),
  request: () => ({ type: OPEN_PROJECT_EDITION.REQUEST }),
  success: (data, healthRisks) => ({ type: OPEN_PROJECT_EDITION.SUCCESS, data, healthRisks }),
  failure: (message) => ({ type: OPEN_PROJECT_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (nationalSocietyId, projectId, data) => ({ type: EDIT_PROJECT.INVOKE, nationalSocietyId, projectId, data }),
  request: () => ({ type: EDIT_PROJECT.REQUEST }),
  success: () => ({ type: EDIT_PROJECT.SUCCESS }),
  failure: (message) => ({ type: EDIT_PROJECT.FAILURE, message })
};

export const remove = {
  invoke: (nationalSocietyId, projectId) => ({ type: REMOVE_PROJECT.INVOKE, nationalSocietyId, projectId }),
  request: (id) => ({ type: REMOVE_PROJECT.REQUEST, id }),
  success: (id) => ({ type: REMOVE_PROJECT.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_PROJECT.FAILURE, id, message })
};

export const openDashbaord = {
  invoke: (projectId) => ({ type: OPEN_PROJECT_DASHBOARD.INVOKE, projectId }),
  request: () => ({ type: OPEN_PROJECT_DASHBOARD.REQUEST }),
  success: (name) => ({ type: OPEN_PROJECT_DASHBOARD.SUCCESS, name }),
  failure: (message) => ({ type: OPEN_PROJECT_DASHBOARD.FAILURE, message })
};