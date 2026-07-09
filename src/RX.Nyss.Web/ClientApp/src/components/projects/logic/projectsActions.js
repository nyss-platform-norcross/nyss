import { push } from "connected-react-router";
import {
  OPEN_PROJECTS_LIST,
  GET_PROJECTS,
  OPEN_PROJECT_CREATION,
  CREATE_PROJECT,
  OPEN_PROJECT_EDITION,
  EDIT_PROJECT,
  EDIT_PROJECT_HEALTH_RISKS,
  CLOSE_PROJECT,
  OPEN_PROJECT_OVERVIEW,
  OPEN_PROJECT_HEALTH_RISKS_OVERVIEW,
  OPEN_ERROR_MESSAGES,
  OPEN_PROJECT_HEALTHRISKS_EDITION,
} from "./projectsConstants";

export const goToList = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/projects`);
export const goToCreation = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/projects/add`);
export const goToEdition = (nationalSocietyId, projectId) =>
  push(`/nationalsocieties/${nationalSocietyId}/projects/${projectId}/edit`);
export const goToDashboard = (nationalSocietyId, projectId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/projects/${projectId}/dashboard`,
  );
export const goToOverview = (nationalSocietyId, projectId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/projects/${projectId}/overview`,
  );
export const goToHealthRisksEdition = (nationalSocietyId, projectId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/projects/${projectId}/editHealthRisks`,
  );
export const goToHealthRisks = (nationalSocietyId, projectId) =>
  push(
    `/nationalsocieties/${nationalSocietyId}/projects/${projectId}/healthrisks`,
  );

export const openList = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_PROJECTS_LIST.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_PROJECTS_LIST.REQUEST }),
  success: () => ({ type: OPEN_PROJECTS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_PROJECTS_LIST.FAILURE, message }),
};

export const getList = {
  invoke: (nationalSocietyId) => ({
    type: GET_PROJECTS.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: GET_PROJECTS.REQUEST }),
  success: (list) => ({ type: GET_PROJECTS.SUCCESS, list }),
  failure: (message) => ({ type: GET_PROJECTS.FAILURE, message }),
};

export const openCreation = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_PROJECT_CREATION.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_PROJECT_CREATION.REQUEST }),
  success: (data) => ({ type: OPEN_PROJECT_CREATION.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_PROJECT_CREATION.FAILURE, message }),
};

export const create = {
  invoke: (nationalSocietyId, data) => ({
    type: CREATE_PROJECT.INVOKE,
    nationalSocietyId,
    data,
  }),
  request: () => ({ type: CREATE_PROJECT.REQUEST }),
  success: () => ({ type: CREATE_PROJECT.SUCCESS }),
  failure: (error) => ({
    type: CREATE_PROJECT.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: (nationalSocietyId, projectId) => ({
    type: OPEN_PROJECT_EDITION.INVOKE,
    nationalSocietyId,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_EDITION.REQUEST }),
  success: (data, healthRisks, timeZones) => ({
    type: OPEN_PROJECT_EDITION.SUCCESS,
    data,
    healthRisks,
    timeZones,
  }),
  failure: (message) => ({ type: OPEN_PROJECT_EDITION.FAILURE, message }),
};

export const openHealthRisksEdition = {
  invoke: (nationalSocietyId, projectId) => ({
    type: OPEN_PROJECT_HEALTHRISKS_EDITION.INVOKE,
    nationalSocietyId,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_HEALTHRISKS_EDITION.REQUEST }),
  success: (data, healthRisks, timeZones) => ({
    type: OPEN_PROJECT_HEALTHRISKS_EDITION.SUCCESS,
    data,
    healthRisks,
    timeZones,
  }),
  failure: (message) => ({
    type: OPEN_PROJECT_HEALTHRISKS_EDITION.FAILURE,
    message,
  }),
};

export const openOverview = {
  invoke: (nationalSocietyId, projectId) => ({
    type: OPEN_PROJECT_OVERVIEW.INVOKE,
    nationalSocietyId,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_OVERVIEW.REQUEST }),
  success: (data, healthRisks, timeZones) => ({
    type: OPEN_PROJECT_OVERVIEW.SUCCESS,
    data,
    healthRisks,
    timeZones,
  }),
  failure: (message) => ({ type: OPEN_PROJECT_OVERVIEW.FAILURE, message }),
};

export const openHealthRisksOverview = {
  invoke: (nationalSocietyId, projectId) => ({
    type: OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.INVOKE,
    nationalSocietyId,
    projectId,
  }),
  request: () => ({ type: OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.REQUEST }),
  success: (data, healthRisks, timeZones) => ({
    type: OPEN_PROJECT_HEALTH_RISKS_OVERVIEW.SUCCESS,
    data,
    healthRisks,
    timeZones,
  }),
  failure: (message) => ({ type: OPEN_PROJECT_OVERVIEW.FAILURE, message }),
};

export const edit = {
  invoke: (nationalSocietyId, projectId, data) => ({
    type: EDIT_PROJECT.INVOKE,
    nationalSocietyId,
    projectId,
    data,
  }),
  request: () => ({ type: EDIT_PROJECT.REQUEST }),
  success: () => ({ type: EDIT_PROJECT.SUCCESS }),
  failure: (error) => ({
    type: EDIT_PROJECT.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const editHealthRisks = {
  invoke: (nationalSocietyId, projectId, healthRisks) => ({
    type: EDIT_PROJECT_HEALTH_RISKS.INVOKE,
    nationalSocietyId,
    projectId,
    healthRisks,
  }),
  request: () => ({ type: EDIT_PROJECT_HEALTH_RISKS.REQUEST }),
  success: () => ({ type: EDIT_PROJECT_HEALTH_RISKS.SUCCESS }),
  failure: (error) => ({
    type: EDIT_PROJECT_HEALTH_RISKS.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const close = {
  invoke: (nationalSocietyId, projectId) => ({
    type: CLOSE_PROJECT.INVOKE,
    nationalSocietyId,
    projectId,
  }),
  request: (id) => ({ type: CLOSE_PROJECT.REQUEST, id }),
  success: (id) => ({ type: CLOSE_PROJECT.SUCCESS, id }),
  failure: (id, message) => ({ type: CLOSE_PROJECT.FAILURE, id, message }),
};

export const openErrorMessages = (projectId) => ({
  type: OPEN_ERROR_MESSAGES,
  projectId,
});
