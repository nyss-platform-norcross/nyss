import { push } from "connected-react-router";
import {
  GET_EIDSR_INTEGRATION,
  EDIT_EIDSR_INTEGRATION,
  GET_EIDSR_ORGANISATION_UNITS,
  GET_EIDSR_PROGRAM,
} from "./eidsrIntegrationConstants";

export const goToEidsrIntegration = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/eidsrintegration`);
export const goToEidsrIntegrationEdition = (nationalSocietyId) =>
  push(`/nationalsocieties/${nationalSocietyId}/eidsrintegration/edit`);

export const get = {
  invoke: (nationalSocietyId) => ({
    type: GET_EIDSR_INTEGRATION.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: GET_EIDSR_INTEGRATION.REQUEST }),
  success: (data) => ({ type: GET_EIDSR_INTEGRATION.SUCCESS, data }),
  failure: (message) => ({ type: GET_EIDSR_INTEGRATION.FAILURE, message }),
};

export const edit = {
  invoke: (id, data) => ({ type: EDIT_EIDSR_INTEGRATION.INVOKE, id, data }),
  request: () => ({ type: EDIT_EIDSR_INTEGRATION.REQUEST }),
  success: () => ({ type: EDIT_EIDSR_INTEGRATION.SUCCESS }),
  failure: (error) => ({
    type: EDIT_EIDSR_INTEGRATION.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const getOrganisationUnits = {
  invoke: (eidsrApiProperties, programId) => ({
    type: GET_EIDSR_ORGANISATION_UNITS.INVOKE,
    eidsrApiProperties,
    programId,
  }),
  request: () => ({ type: GET_EIDSR_ORGANISATION_UNITS.REQUEST }),
  success: (organisationUnits) => ({
    type: GET_EIDSR_ORGANISATION_UNITS.SUCCESS,
    organisationUnits,
  }),
  failure: (message) => ({
    type: GET_EIDSR_ORGANISATION_UNITS.FAILURE,
    message,
  }),
};

export const getProgram = {
  invoke: (eidsrApiProperties, programId) => ({
    type: GET_EIDSR_PROGRAM.INVOKE,
    eidsrApiProperties,
    programId,
  }),
  request: () => ({ type: GET_EIDSR_PROGRAM.REQUEST }),
  success: (program) => ({ type: GET_EIDSR_PROGRAM.SUCCESS, program }),
  failure: (message) => ({ type: GET_EIDSR_PROGRAM.FAILURE, message }),
};
