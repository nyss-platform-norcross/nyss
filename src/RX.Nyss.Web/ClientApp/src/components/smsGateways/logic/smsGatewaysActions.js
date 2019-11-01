import { push } from "connected-react-router";
import { 
  OPEN_SMS_GATEWAYS_LIST, GET_SMS_GATEWAYS,
  OPEN_CREATION_SMS_GATEWAY, CREATE_SMS_GATEWAY,
  OPEN_EDITION_SMS_GATEWAY, EDIT_SMS_GATEWAY,
  REMOVE_SMS_GATEWAY 
} from "./smsGatewaysConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways`);
export const goToCreation = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/add`);
export const goToEdition = (nationalSocietyId, smsGatewayId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/${smsGatewayId}/edit`);

export const openList = {
  invoke: (path, params) => ({ type: OPEN_SMS_GATEWAYS_LIST.INVOKE, path, params }),
  request: () => ({ type: OPEN_SMS_GATEWAYS_LIST.REQUEST }),
  success: () => ({ type: OPEN_SMS_GATEWAYS_LIST.SUCCESS }),
  failure: (message) => ({ type: OPEN_SMS_GATEWAYS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (nationalSocietyId) => ({ type: GET_SMS_GATEWAYS.INVOKE, nationalSocietyId }),
  request: () => ({ type: GET_SMS_GATEWAYS.REQUEST }),
  success: (list) => ({ type: GET_SMS_GATEWAYS.SUCCESS, list }),
  failure: (message) => ({ type: GET_SMS_GATEWAYS.FAILURE, message })
};

export const openCreation = {
  invoke: (path, params) => ({ type: OPEN_CREATION_SMS_GATEWAY.INVOKE, path, params }),
  request: () => ({ type: OPEN_CREATION_SMS_GATEWAY.REQUEST }),
  success: () => ({ type: OPEN_CREATION_SMS_GATEWAY.SUCCESS }),
  failure: (message) => ({ type: OPEN_CREATION_SMS_GATEWAY.FAILURE, message })
};

export const create = {
  invoke: (nationalSocietyId, data) => ({ type: CREATE_SMS_GATEWAY.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: CREATE_SMS_GATEWAY.REQUEST }),
  success: () => ({ type: CREATE_SMS_GATEWAY.SUCCESS }),
  failure: (message) => ({ type: CREATE_SMS_GATEWAY.FAILURE, message })
};

export const openEdition = {
  invoke: (path, params) => ({ type: OPEN_EDITION_SMS_GATEWAY.INVOKE, path, params }),
  request: () => ({ type: OPEN_EDITION_SMS_GATEWAY.REQUEST }),
  success: (data) => ({ type: OPEN_EDITION_SMS_GATEWAY.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_EDITION_SMS_GATEWAY.FAILURE, message })
};

export const edit = {
  invoke: (data) => ({ type: EDIT_SMS_GATEWAY.INVOKE, data }),
  request: () => ({ type: EDIT_SMS_GATEWAY.REQUEST }),
  success: () => ({ type: EDIT_SMS_GATEWAY.SUCCESS }),
  failure: (message) => ({ type: EDIT_SMS_GATEWAY.FAILURE, message })
};

export const remove = {
  invoke: (smsGatewayId, nationalSocietyId) => ({ type: REMOVE_SMS_GATEWAY.INVOKE, smsGatewayId, nationalSocietyId }),
  request: (id) => ({ type: REMOVE_SMS_GATEWAY.REQUEST, id }),
  success: (id) => ({ type: REMOVE_SMS_GATEWAY.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_SMS_GATEWAY.FAILURE, id, message })
};