import { push } from "connected-react-router";
import {
  OPEN_SMS_GATEWAYS_LIST, GET_SMS_GATEWAYS,
  OPEN_SMS_GATEWAY_CREATION, CREATE_SMS_GATEWAY,
  OPEN_SMS_GATEWAY_EDITION, EDIT_SMS_GATEWAY,
  REMOVE_SMS_GATEWAY
} from "./smsGatewaysConstants";

export const goToList = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways`);
export const goToCreation = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/add`);
export const goToEdition = (nationalSocietyId, smsGatewayId) => push(`/nationalsocieties/${nationalSocietyId}/smsgateways/${smsGatewayId}/edit`);

export const openList = {
  invoke: (nationalSocietyId) => ({ type: OPEN_SMS_GATEWAYS_LIST.INVOKE, nationalSocietyId }),
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
  invoke: (nationalSocietyId) => ({ type: OPEN_SMS_GATEWAY_CREATION.INVOKE, nationalSocietyId }),
  request: () => ({ type: OPEN_SMS_GATEWAY_CREATION.REQUEST }),
  success: () => ({ type: OPEN_SMS_GATEWAY_CREATION.SUCCESS }),
  failure: (message) => ({ type: OPEN_SMS_GATEWAY_CREATION.FAILURE, message })
};

export const create = {
  invoke: (nationalSocietyId, data) => ({ type: CREATE_SMS_GATEWAY.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: CREATE_SMS_GATEWAY.REQUEST }),
  success: () => ({ type: CREATE_SMS_GATEWAY.SUCCESS }),
  failure: (message) => ({ type: CREATE_SMS_GATEWAY.FAILURE, message })
};

export const openEdition = {
  invoke: (smsGatewayId) => ({ type: OPEN_SMS_GATEWAY_EDITION.INVOKE, smsGatewayId }),
  request: () => ({ type: OPEN_SMS_GATEWAY_EDITION.REQUEST }),
  success: (data) => ({ type: OPEN_SMS_GATEWAY_EDITION.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_SMS_GATEWAY_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (nationalSocietyId, data) => ({ type: EDIT_SMS_GATEWAY.INVOKE, nationalSocietyId, data }),
  request: () => ({ type: EDIT_SMS_GATEWAY.REQUEST }),
  success: () => ({ type: EDIT_SMS_GATEWAY.SUCCESS }),
  failure: (message) => ({ type: EDIT_SMS_GATEWAY.FAILURE, message })
};

export const remove = {
  invoke: (smsGatewayId) => ({ type: REMOVE_SMS_GATEWAY.INVOKE, smsGatewayId }),
  request: (id) => ({ type: REMOVE_SMS_GATEWAY.REQUEST, id }),
  success: (id) => ({ type: REMOVE_SMS_GATEWAY.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_SMS_GATEWAY.FAILURE, id, message })
};