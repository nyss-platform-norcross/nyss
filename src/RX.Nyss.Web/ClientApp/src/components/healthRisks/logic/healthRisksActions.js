import { push } from "connected-react-router";
import { GET_HEALTH_RISKS, CREATE_HEALTH_RISK, REMOVE_HEALTH_RISK } from "./healthRisksConstants";
import { OPEN_EDITION_HEALTH_RISK, EDIT_HEALTH_RISK } from "./healthRisksConstants";

export const goToCreation = () => push("/healthrisks/add");
export const goToList = () => push("/healthrisks");
export const goToEdition = (id) => push(`/healthrisks/${id}/edit`);

export const getList = {
  invoke: () => ({ type: GET_HEALTH_RISKS.INVOKE }),
  request: () => ({ type: GET_HEALTH_RISKS.REQUEST }),
  success: (list) => ({ type: GET_HEALTH_RISKS.SUCCESS, list }),
  failure: (message) => ({ type: GET_HEALTH_RISKS.FAILURE, message })
};

export const create = {
  invoke: (data) => ({ type: CREATE_HEALTH_RISK.INVOKE, data }),
  request: () => ({ type: CREATE_HEALTH_RISK.REQUEST }),
  success: () => ({ type: CREATE_HEALTH_RISK.SUCCESS }),
  failure: (message) => ({ type: CREATE_HEALTH_RISK.FAILURE, message })
};

export const edit = {
  invoke: (data) => ({ type: EDIT_HEALTH_RISK.INVOKE, data }),
  request: () => ({ type: EDIT_HEALTH_RISK.REQUEST }),
  success: () => ({ type: EDIT_HEALTH_RISK.SUCCESS }),
  failure: (message) => ({ type: EDIT_HEALTH_RISK.FAILURE, message })
};

export const openEdition = {
  invoke: ({ path, params }) => ({ type: OPEN_EDITION_HEALTH_RISK.INVOKE, path, params }),
  request: () => ({ type: OPEN_EDITION_HEALTH_RISK.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_HEALTH_RISK.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_EDITION_HEALTH_RISK.FAILURE, message })
};

export const remove = {
  invoke: (id) => ({ type: REMOVE_HEALTH_RISK.INVOKE, id }),
  request: (id) => ({ type: REMOVE_HEALTH_RISK.REQUEST, id }),
  success: (id) => ({ type: REMOVE_HEALTH_RISK.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_HEALTH_RISK.FAILURE, id, message })
};