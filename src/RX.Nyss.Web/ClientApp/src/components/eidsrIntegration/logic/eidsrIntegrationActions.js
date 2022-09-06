import { push } from "connected-react-router";
import {
  GET_EIDSR_INTEGRATION, EDIT_EIDSR_INTEGRATION
} from "./eidsrIntegrationConstants";

export const goToEidsrIntegration = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/eidsrintegration`);
export const goToEidsrIntegrationEdition = (nationalSocietyId) => push(`/nationalsocieties/${nationalSocietyId}/eidsrintegration/edit`);

export const get = {
  invoke: (nationalSocietyId) => ({ type: GET_EIDSR_INTEGRATION.INVOKE, nationalSocietyId }),
  request: () => ({ type: GET_EIDSR_INTEGRATION.REQUEST }),
  success: (data) => ({ type: GET_EIDSR_INTEGRATION.SUCCESS, data }),
  failure: (message) => ({ type: GET_EIDSR_INTEGRATION.FAILURE, message })
};

export const edit = {
  invoke: (data) => ({ type: EDIT_EIDSR_INTEGRATION.INVOKE, data }),
  request: () => ({ type: EDIT_EIDSR_INTEGRATION.REQUEST }),
  success: () => ({ type: EDIT_EIDSR_INTEGRATION.SUCCESS }),
  failure: (error) => ({ type: EDIT_EIDSR_INTEGRATION.FAILURE, error, suppressPopup: true })
};
