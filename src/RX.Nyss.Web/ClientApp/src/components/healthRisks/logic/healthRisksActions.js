import { push } from "connected-react-router";
import {
  GET_HEALTH_RISKS,
  CREATE_HEALTH_RISK,
  REMOVE_HEALTH_RISK,
  OPEN_CREATION_HEALTH_RISK,
} from "./healthRisksConstants";
import {
  OPEN_EDITION_HEALTH_RISK,
  EDIT_HEALTH_RISK,
} from "./healthRisksConstants";

export const goToCreation = () => push("/healthrisks/add"); /// for listing page
export const goToList = () => push("/healthrisks");
export const goToEdition = (id) => push(`/healthrisks/${id}/edit`);

export const getList = {
  invoke: () => ({ type: GET_HEALTH_RISKS.INVOKE }),
  request: () => ({ type: GET_HEALTH_RISKS.REQUEST }),
  success: (list) => ({ type: GET_HEALTH_RISKS.SUCCESS, list }),
  failure: (message) => ({ type: GET_HEALTH_RISKS.FAILURE, message }),
};

//Added as new creation action to handle start of health risk creation and data we need for that
export const openCreation = {
  invoke: (data) => ({ type: OPEN_CREATION_HEALTH_RISK.INVOKE, data }),
  request: () => ({ type: OPEN_CREATION_HEALTH_RISK.REQUEST }),
  success: (data) => ({ type: OPEN_CREATION_HEALTH_RISK.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_CREATION_HEALTH_RISK.FAILURE, message }),
};

export const create = {
  invoke: (data) => ({ type: CREATE_HEALTH_RISK.INVOKE, data }),
  request: () => ({ type: CREATE_HEALTH_RISK.REQUEST }),
  success: () => ({ type: CREATE_HEALTH_RISK.SUCCESS }),
  failure: (error) => ({
    type: CREATE_HEALTH_RISK.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (id, data) => ({ type: EDIT_HEALTH_RISK.INVOKE, id, data }),
  request: () => ({ type: EDIT_HEALTH_RISK.REQUEST }),
  success: () => ({ type: EDIT_HEALTH_RISK.SUCCESS }),
  failure: (error) => ({
    type: EDIT_HEALTH_RISK.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const openEdition = {
  invoke: ({ path, params }) => ({
    type: OPEN_EDITION_HEALTH_RISK.INVOKE,
    path,
    params,
  }),
  request: () => ({ type: OPEN_EDITION_HEALTH_RISK.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_HEALTH_RISK.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_EDITION_HEALTH_RISK.FAILURE, message }),
};

export const remove = {
  invoke: (id) => ({ type: REMOVE_HEALTH_RISK.INVOKE, id }),
  request: (id) => ({ type: REMOVE_HEALTH_RISK.REQUEST, id }),
  success: (id) => ({ type: REMOVE_HEALTH_RISK.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_HEALTH_RISK.FAILURE, id, message }),
};
