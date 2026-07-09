import { push } from "connected-react-router";
import {
  OPEN_ALERT_EVENT_LOG,
  GET_ALERT_EVENT_LOG,
  OPEN_ALERT_EVENT_CREATION,
  CREATE_ALERT_EVENT,
  EDIT_ALERT_EVENT,
  DELETE_ALERT_EVENT,
} from "./alertEventsConstants";

export const goToLog = (projectId, alertId) =>
  push(`/projects/${projectId}/alerts/${alertId}/eventLog`);

export const openEventLog = {
  invoke: (projectId, alertId) => ({
    type: OPEN_ALERT_EVENT_LOG.INVOKE,
    projectId,
    alertId,
  }),
  request: () => ({ type: OPEN_ALERT_EVENT_LOG.REQUEST }),
  success: (alertId, data) => ({
    type: OPEN_ALERT_EVENT_LOG.SUCCESS,
    alertId,
    data,
  }),
  failure: (message) => ({ type: OPEN_ALERT_EVENT_LOG.FAILURE, message }),
};

export const getEventLog = {
  invoke: (alertId) => ({ type: GET_ALERT_EVENT_LOG.INVOKE, alertId }),
  request: () => ({ type: GET_ALERT_EVENT_LOG.REQUEST }),
  success: (data) => ({ type: GET_ALERT_EVENT_LOG.SUCCESS, data }),
  failure: (message) => ({ type: GET_ALERT_EVENT_LOG.FAILURE, message }),
};

export const openCreation = {
  invoke: () => ({ type: OPEN_ALERT_EVENT_CREATION.INVOKE }),
  request: () => ({ type: OPEN_ALERT_EVENT_CREATION.REQUEST }),
  success: (alertEventTypes, alertEventSubtypes) => ({
    type: OPEN_ALERT_EVENT_CREATION.SUCCESS,
    alertEventTypes,
    alertEventSubtypes,
  }),
  failure: (message) => ({ type: OPEN_ALERT_EVENT_CREATION.FAILURE, message }),
};

export const create = {
  invoke: (alertId, data) => ({
    type: CREATE_ALERT_EVENT.INVOKE,
    alertId,
    data,
  }),
  request: () => ({ type: CREATE_ALERT_EVENT.REQUEST }),
  success: (formData) => ({ type: CREATE_ALERT_EVENT.SUCCESS, formData }),
  failure: (error) => ({
    type: CREATE_ALERT_EVENT.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const edit = {
  invoke: (alertId, alertEventLogId, text) => ({
    type: EDIT_ALERT_EVENT.INVOKE,
    alertId,
    alertEventLogId,
    text,
  }),
  request: () => ({ type: EDIT_ALERT_EVENT.REQUEST }),
  success: () => ({ type: EDIT_ALERT_EVENT.SUCCESS }),
  failure: (error) => ({
    type: EDIT_ALERT_EVENT.FAILURE,
    error,
    suppressPopup: true,
  }),
};

export const remove = {
  invoke: (alertId, alertEventLogId) => ({
    type: DELETE_ALERT_EVENT.INVOKE,
    alertId,
    alertEventLogId,
  }),
  request: (alertEventLogId) => ({
    type: DELETE_ALERT_EVENT.REQUEST,
    alertEventLogId,
  }),
  success: (alertEventLogId) => ({
    type: DELETE_ALERT_EVENT.SUCCESS,
    alertEventLogId,
  }),
  failure: (id, message) => ({ type: DELETE_ALERT_EVENT.FAILURE, id, message }),
};
