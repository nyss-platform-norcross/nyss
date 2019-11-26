import { push } from "connected-react-router";
import {
  OPEN_DATA_COLLECTORS_LIST, GET_DATA_COLLECTORS,
  OPEN_DATA_COLLECTOR_CREATION, CREATE_DATA_COLLECTOR,
  OPEN_DATA_COLLECTOR_EDITION, EDIT_DATA_COLLECTOR,
  REMOVE_DATA_COLLECTOR, OPEN_DATA_COLLECTORS_MAP_OVERVIEW,
  GET_DATA_COLLECTORS_MAP_OVERVIEW
} from "./dataCollectorsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/datacollectors/list`);
export const goToOverview = (projectId) => push(`/projects/${projectId}/datacollectors/mapoverview`);
export const goToCreation = (projectId) => push(`/projects/${projectId}/datacollectors/add`);
export const goToEdition = (projectId, dataCollectorId) => push(`/projects/${projectId}/datacollectors/${dataCollectorId}/edit`);

export const openList = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTORS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_LIST.REQUEST }),
  success: (projectId) => ({ type: OPEN_DATA_COLLECTORS_LIST.SUCCESS, projectId }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId) => ({ type: GET_DATA_COLLECTORS.INVOKE, projectId }),
  request: () => ({ type: GET_DATA_COLLECTORS.REQUEST }),
  success: (list) => ({ type: GET_DATA_COLLECTORS.SUCCESS, list }),
  failure: (message) => ({ type: GET_DATA_COLLECTORS.FAILURE, message })
};

export const openMapOverview = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.REQUEST }),
  success: (projectId) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.SUCCESS, projectId }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.FAILURE, message })
};

export const getMapOverview = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.REQUEST }),
  success: (list) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.SUCCESS, list }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.FAILURE, message })
};

export const openCreation = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTOR_CREATION.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTOR_CREATION.REQUEST }),
  success: (regions, supervisors, defaultLocation, defaultSupervisorId) => ({ type: OPEN_DATA_COLLECTOR_CREATION.SUCCESS, regions, supervisors, defaultLocation, defaultSupervisorId }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTOR_CREATION.FAILURE, message })
};

export const create = {
  invoke: (projectId, data) => ({ type: CREATE_DATA_COLLECTOR.INVOKE, projectId, data }),
  request: () => ({ type: CREATE_DATA_COLLECTOR.REQUEST }),
  success: () => ({ type: CREATE_DATA_COLLECTOR.SUCCESS }),
  failure: (message) => ({ type: CREATE_DATA_COLLECTOR.FAILURE, message })
};

export const openEdition = {
  invoke: (dataCollectorId) => ({ type: OPEN_DATA_COLLECTOR_EDITION.INVOKE, dataCollectorId }),
  request: () => ({ type: OPEN_DATA_COLLECTOR_EDITION.REQUEST }),
  success: (data) => ({ type: OPEN_DATA_COLLECTOR_EDITION.SUCCESS, data }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTOR_EDITION.FAILURE, message })
};

export const edit = {
  invoke: (projectId, data) => ({ type: EDIT_DATA_COLLECTOR.INVOKE, projectId, data }),
  request: () => ({ type: EDIT_DATA_COLLECTOR.REQUEST }),
  success: () => ({ type: EDIT_DATA_COLLECTOR.SUCCESS }),
  failure: (message) => ({ type: EDIT_DATA_COLLECTOR.FAILURE, message })
};

export const remove = {
  invoke: (dataCollectorId) => ({ type: REMOVE_DATA_COLLECTOR.INVOKE, dataCollectorId }),
  request: (id) => ({ type: REMOVE_DATA_COLLECTOR.REQUEST, id }),
  success: (id) => ({ type: REMOVE_DATA_COLLECTOR.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_DATA_COLLECTOR.FAILURE, id, message })
};
