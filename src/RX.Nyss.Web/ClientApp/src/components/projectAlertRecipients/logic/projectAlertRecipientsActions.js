import { push } from "connected-react-router";
import {
  OPEN_ALERT_RECIPIENTS_LIST,
  GET_ALERT_RECIPIENTS,
  OPEN_ALERT_RECIPIENT_CREATION,
  CREATE_ALERT_RECIPIENT,
  REMOVE_ALERT_RECIPIENT,
  OPEN_ALERT_RECIPIENT_EDITION,
  EDIT_ALERT_RECIPIENT,
} from "./projectAlertRecipientsConstants";

export const goToList = (projectId) =>
  push(`/projects/${projectId}/escalatedAlertNotifications`);
export const goToCreation = (projectId) =>
  push(`/projects/${projectId}/escalatedAlertNotifications/addRecipient`);
export const goToEdition = (projectId, alertRecipientId) =>
  push(
    `/projects/${projectId}/escalatedAlertNotifications/${alertRecipientId}/editRecipient`,
  );

export const openList = {
  invoke: (projectId) => ({
    type: OPEN_ALERT_RECIPIENTS_LIST.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_ALERT_RECIPIENTS_LIST.REQUEST }),
  success: (projectId) => ({
    type: OPEN_ALERT_RECIPIENTS_LIST.SUCCESS,
    projectId,
  }),
  failure: (message) => ({ type: OPEN_ALERT_RECIPIENTS_LIST.FAILURE, message }),
};

export const getList = {
  invoke: (projectId) => ({ type: GET_ALERT_RECIPIENTS.INVOKE, projectId }),
  request: () => ({ type: GET_ALERT_RECIPIENTS.REQUEST }),
  success: (list) => ({ type: GET_ALERT_RECIPIENTS.SUCCESS, list }),
  failure: (message) => ({ type: GET_ALERT_RECIPIENTS.FAILURE, message }),
};

export const openCreation = {
  invoke: (projectId) => ({
    type: OPEN_ALERT_RECIPIENT_CREATION.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_ALERT_RECIPIENT_CREATION.REQUEST }),
  success: (formData) => ({
    type: OPEN_ALERT_RECIPIENT_CREATION.SUCCESS,
    formData,
  }),
  failure: (message) => ({
    type: OPEN_ALERT_RECIPIENT_CREATION.FAILURE,
    message,
  }),
};

export const openEdition = {
  invoke: (projectId, alertRecipientId) => ({
    type: OPEN_ALERT_RECIPIENT_EDITION.INVOKE,
    projectId,
    alertRecipientId,
  }),
  request: () => ({ type: OPEN_ALERT_RECIPIENT_EDITION.REQUEST }),
  success: (recipient, formData) => ({
    type: OPEN_ALERT_RECIPIENT_EDITION.SUCCESS,
    recipient,
    formData,
  }),
  failure: (message) => ({
    type: OPEN_ALERT_RECIPIENT_EDITION.FAILURE,
    message,
  }),
};

export const create = {
  invoke: (projectId, data) => ({
    type: CREATE_ALERT_RECIPIENT.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: CREATE_ALERT_RECIPIENT.REQUEST }),
  success: () => ({ type: CREATE_ALERT_RECIPIENT.SUCCESS }),
  failure: (message) => ({
    type: CREATE_ALERT_RECIPIENT.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (projectId, data) => ({
    type: EDIT_ALERT_RECIPIENT.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: EDIT_ALERT_RECIPIENT.REQUEST }),
  success: () => ({ type: EDIT_ALERT_RECIPIENT.SUCCESS }),
  failure: (message) => ({
    type: EDIT_ALERT_RECIPIENT.FAILURE,
    message,
    suppressPopup: true,
  }),
};

export const remove = {
  invoke: (projectId, alertRecipientId) => ({
    type: REMOVE_ALERT_RECIPIENT.INVOKE,
    projectId,
    alertRecipientId,
  }),
  request: (id) => ({ type: REMOVE_ALERT_RECIPIENT.REQUEST, id }),
  success: (id) => ({ type: REMOVE_ALERT_RECIPIENT.SUCCESS, id }),
  failure: (id, message) => ({
    type: REMOVE_ALERT_RECIPIENT.FAILURE,
    id,
    message,
  }),
};
