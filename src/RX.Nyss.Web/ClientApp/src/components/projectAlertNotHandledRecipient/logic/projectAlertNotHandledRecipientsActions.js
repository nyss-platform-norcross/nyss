import {
  CREATE_ALERT_NOT_HANDLED_RECIPIENT,
  EDIT_ALERT_NOT_HANDLED_RECIPIENT,
  REMOVE_ALERT_NOT_HANDLED_RECIPIENT,
  GET_ALERT_NOT_HANDLED_FORM_DATA,
  GET_ALERT_NOT_HANDLED_RECIPIENTS,
  OPEN_ALERT_NOT_HANDLED_RECIPIENTS,
} from "./projectAlertNotHandledRecipientsConstants";

export const openRecipients = {
  invoke: (projectId) => ({
    type: OPEN_ALERT_NOT_HANDLED_RECIPIENTS.INVOKE,
    projectId,
  }),
  request: () => ({ type: OPEN_ALERT_NOT_HANDLED_RECIPIENTS.REQUEST }),
  success: (projectId) => ({
    type: OPEN_ALERT_NOT_HANDLED_RECIPIENTS.SUCCESS,
    projectId,
  }),
  failure: (message) => ({
    type: OPEN_ALERT_NOT_HANDLED_RECIPIENTS.FAILURE,
    message,
  }),
};

export const getRecipients = {
  invoke: (projectId) => ({
    type: GET_ALERT_NOT_HANDLED_RECIPIENTS.INVOKE,
    projectId,
  }),
  request: () => ({ type: GET_ALERT_NOT_HANDLED_RECIPIENTS.REQUEST }),
  success: (recipients) => ({
    type: GET_ALERT_NOT_HANDLED_RECIPIENTS.SUCCESS,
    recipients,
  }),
  failure: (message) => ({
    type: GET_ALERT_NOT_HANDLED_RECIPIENTS.FAILURE,
    message,
  }),
};

export const create = {
  invoke: (projectId, data) => ({
    type: CREATE_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: CREATE_ALERT_NOT_HANDLED_RECIPIENT.REQUEST }),
  success: () => ({ type: CREATE_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS }),
  failure: (message) => ({
    type: CREATE_ALERT_NOT_HANDLED_RECIPIENT.FAILURE,
    message,
  }),
};

export const edit = {
  invoke: (projectId, data) => ({
    type: EDIT_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: EDIT_ALERT_NOT_HANDLED_RECIPIENT.REQUEST }),
  success: () => ({ type: EDIT_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS }),
  failure: (message) => ({
    type: EDIT_ALERT_NOT_HANDLED_RECIPIENT.FAILURE,
    message,
  }),
};

export const remove = {
  invoke: (projectId, data) => ({
    type: REMOVE_ALERT_NOT_HANDLED_RECIPIENT.INVOKE,
    projectId,
    data,
  }),
  request: () => ({ type: REMOVE_ALERT_NOT_HANDLED_RECIPIENT.REQUEST }),
  success: () => ({ type: REMOVE_ALERT_NOT_HANDLED_RECIPIENT.SUCCESS }),
  failure: (message) => ({
    type: REMOVE_ALERT_NOT_HANDLED_RECIPIENT.FAILURE,
    message,
  }),
};

export const getFormData = {
  invoke: (projectId) => ({
    type: GET_ALERT_NOT_HANDLED_FORM_DATA.INVOKE,
    projectId,
  }),
  request: () => ({ type: GET_ALERT_NOT_HANDLED_FORM_DATA.REQUEST }),
  success: (users) => ({
    type: GET_ALERT_NOT_HANDLED_FORM_DATA.SUCCESS,
    users,
  }),
  failure: (message) => ({
    type: GET_ALERT_NOT_HANDLED_FORM_DATA.FAILURE,
    message,
  }),
};
