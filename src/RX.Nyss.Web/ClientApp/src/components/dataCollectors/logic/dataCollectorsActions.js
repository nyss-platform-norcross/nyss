import { push } from "connected-react-router";
import {
  OPEN_DATA_COLLECTORS_LIST, GET_DATA_COLLECTORS,
  OPEN_DATA_COLLECTOR_CREATION, CREATE_DATA_COLLECTOR,
  OPEN_DATA_COLLECTOR_EDITION, EDIT_DATA_COLLECTOR,
  REMOVE_DATA_COLLECTOR, OPEN_DATA_COLLECTORS_MAP_OVERVIEW,
  GET_DATA_COLLECTORS_MAP_OVERVIEW,
  GET_DATA_COLLECTORS_MAP_DETAILS,
  SET_DATA_COLLECTORS_TRAINING_STATE,
  OPEN_DATA_COLLECTORS_PERFORMANCE_LIST,
  GET_DATA_COLLECTORS_PERFORMANCE,
  EXPORT_DATA_COLLECTORS_TO_CSV,
  EXPORT_DATA_COLLECTORS_TO_EXCEL,
  SELECT_DATA_COLLECTOR,
  SELECT_ALL_DATA_COLLECTOR,
  REPLACE_SUPERVISOR
} from "./dataCollectorsConstants";

export const goToList = (projectId) => push(`/projects/${projectId}/datacollectors/list`);
export const goToOverview = (projectId) => push(`/projects/${projectId}/datacollectors/mapoverview`);
export const goToCreation = (projectId) => push(`/projects/${projectId}/datacollectors/add`);
export const goToEdition = (projectId, dataCollectorId) => push(`/projects/${projectId}/datacollectors/${dataCollectorId}/edit`);
export const selectDataCollector = (dataCollectorId, value) => ({ type: SELECT_DATA_COLLECTOR, dataCollectorId, value })
export const selectAllDataCollectors = (value) => ({ type: SELECT_ALL_DATA_COLLECTOR, value })

export const openList = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTORS_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_LIST.REQUEST }),
  success: (projectId, filtersData) => ({ type: OPEN_DATA_COLLECTORS_LIST.SUCCESS, projectId, filtersData }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_LIST.FAILURE, message })
};

export const getList = {
  invoke: (projectId, filters) => ({ type: GET_DATA_COLLECTORS.INVOKE, projectId, filters }),
  request: () => ({ type: GET_DATA_COLLECTORS.REQUEST }),
  success: (list, filters) => ({ type: GET_DATA_COLLECTORS.SUCCESS, list, filters }),
  failure: (message) => ({ type: GET_DATA_COLLECTORS.FAILURE, message })
};

export const openMapOverview = {
  invoke: (projectId, from, to) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, projectId, from, to }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.REQUEST }),
  success: () => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.SUCCESS }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_MAP_OVERVIEW.FAILURE, message })
};

export const getMapOverview = {
  invoke: (projectId, filters) => ({ type: GET_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, projectId, filters }),
  request: () => ({ type: GET_DATA_COLLECTORS_MAP_OVERVIEW.REQUEST }),
  success: (filters, dataCollectorLocations, centerLocation) => ({ type: GET_DATA_COLLECTORS_MAP_OVERVIEW.SUCCESS, filters, dataCollectorLocations, centerLocation }),
  failure: (message) => ({ type: GET_DATA_COLLECTORS_MAP_OVERVIEW.FAILURE, message })
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
  failure: (error) => ({ type: CREATE_DATA_COLLECTOR.FAILURE, error, suppressPopup: true })
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
  failure: (error) => ({ type: EDIT_DATA_COLLECTOR.FAILURE, error, suppressPopup: true })
};

export const remove = {
  invoke: (dataCollectorId) => ({ type: REMOVE_DATA_COLLECTOR.INVOKE, dataCollectorId }),
  request: (id) => ({ type: REMOVE_DATA_COLLECTOR.REQUEST, id }),
  success: (id) => ({ type: REMOVE_DATA_COLLECTOR.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_DATA_COLLECTOR.FAILURE, id, message })
};

export const getMapDetails = {
  invoke: (projectId, lat, lng) => ({ type: GET_DATA_COLLECTORS_MAP_DETAILS.INVOKE, projectId, lat, lng }),
  request: () => ({ type: GET_DATA_COLLECTORS_MAP_DETAILS.REQUEST }),
  success: (details) => ({ type: GET_DATA_COLLECTORS_MAP_DETAILS.SUCCESS, details }),
  failure: (message) => ({ type: GET_DATA_COLLECTORS_MAP_DETAILS.FAILURE, message })
};

export const setTrainingState = {
  invoke: (dataCollectorIds, inTraining) => ({ type: SET_DATA_COLLECTORS_TRAINING_STATE.INVOKE, dataCollectorIds, inTraining }),
  request: (dataCollectorIds) => ({ type: SET_DATA_COLLECTORS_TRAINING_STATE.REQUEST, dataCollectorIds }),
  success: (dataCollectorIds, inTraining) => ({ type: SET_DATA_COLLECTORS_TRAINING_STATE.SUCCESS, dataCollectorIds, inTraining }),
  failure: (dataCollectorIds, message) => ({ type: SET_DATA_COLLECTORS_TRAINING_STATE.FAILURE, dataCollectorIds, message })
};

export const openDataCollectorsPerformanceList = {
  invoke: (projectId) => ({ type: OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.INVOKE, projectId }),
  request: () => ({ type: OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.REQUEST }),
  success: (filtersData) => ({ type: OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.SUCCESS, filtersData }),
  failure: (message) => ({ type: OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.FAILURE, message })
}

export const getDataCollectorsPerformanceList = {
  invoke: (projectId, filters) => ({ type: GET_DATA_COLLECTORS_PERFORMANCE.INVOKE, projectId, filters }),
  request: () => ({ type: GET_DATA_COLLECTORS_PERFORMANCE.REQUEST }),
  success: (list, completeness, filters) => ({ type: GET_DATA_COLLECTORS_PERFORMANCE.SUCCESS, list, completeness, filters }),
  failure: (message) => ({ type: GET_DATA_COLLECTORS_PERFORMANCE.FAILURE, message })
}

export const exportToExcel = {
  invoke: (projectId, filters) => ({ type: EXPORT_DATA_COLLECTORS_TO_EXCEL.INVOKE, projectId, filters }),
  request: () => ({ type: EXPORT_DATA_COLLECTORS_TO_EXCEL.REQUEST }),
  success: () => ({ type: EXPORT_DATA_COLLECTORS_TO_EXCEL.SUCCESS }),
  failure: (message) => ({ type: EXPORT_DATA_COLLECTORS_TO_EXCEL.FAILURE, message })
};

export const exportToCsv = {
  invoke: (projectId, filters) => ({ type: EXPORT_DATA_COLLECTORS_TO_CSV.INVOKE, projectId, filters }),
  request: () => ({ type: EXPORT_DATA_COLLECTORS_TO_CSV.REQUEST }),
  success: () => ({ type: EXPORT_DATA_COLLECTORS_TO_CSV.SUCCESS }),
  failure: (message) => ({ type: EXPORT_DATA_COLLECTORS_TO_CSV.FAILURE, message })
};

export const replaceSupervisor = {
  invoke: (dataCollectorIds, supervisorId) => ({ type: REPLACE_SUPERVISOR.INVOKE, dataCollectorIds, supervisorId }),
  request: (dataCollectorIds) => ({ type: REPLACE_SUPERVISOR.REQUEST, dataCollectorIds }),
  success: (dataCollectorIds) => ({ type: REPLACE_SUPERVISOR.SUCCESS, dataCollectorIds }),
  failure: (dataCollectorIds, error) => ({ type: REPLACE_SUPERVISOR.FAILURE, error, dataCollectorIds })
}
