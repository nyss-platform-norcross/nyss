import * as actions from "./dataCollectorsConstants";
import * as projectsActions from "../../projects/logic/projectsConstants";
import { initialState } from "../../../initialState";
import { setProperty, removeProperty, assignInArray } from "../../../utils/immutable";
import { LOCATION_CHANGE } from "connected-react-router";

const filterIsSet = (filters) => {
  const initialFilters = initialState.dataCollectors.filters;
  return filters.name !== initialFilters.name
  || filters.area !== initialFilters.area
  || filters.supervisorId !== initialFilters.supervisorId
  || filters.sex !== initialFilters.sex
  || filters.pageNumber !== initialFilters.pageNumber
  || filters.trainingStatus !== initialFilters.trainingStatus;
}

export function dataCollectorsReducer(state = initialState.dataCollectors, action) {
  switch (action.type) {
    case projectsActions.CLOSE_PROJECT.SUCCESS:
      return { ...state, listStale: true };

    case LOCATION_CHANGE: // cleanup
      return { ...state, formData: null, formError: null, formDefaultSupervisorId: null, formRegions: [], formSupervisors: [], formDefaultLocation: null };

    case actions.OPEN_DATA_COLLECTORS_LIST.INVOKE:
      return { ...state, filters: action.projectId === state.projectId ? state.filters : initialState.dataCollectors.filters, listData: action.projectId === state.projectId ? state.listData : { data: [], page: null, rowsPerPage: null, totalRows: null } };

    case actions.OPEN_DATA_COLLECTORS_LIST.SUCCESS:
      return { ...state, projectId: action.projectId, filtersData: action.filtersData };

    case actions.GET_DATA_COLLECTORS.REQUEST:
      return { ...state, listFetching: true };

    case actions.GET_DATA_COLLECTORS.SUCCESS:
      return { ...state, listFetching: false, filters: action.filters, listData: action.list, listStale: false, listSelectedAll: false };

    case actions.GET_DATA_COLLECTORS.FAILURE:
      return { ...state, listFetching: false };

    case actions.SELECT_DATA_COLLECTOR:
      return {
        ...state,
        listSelectedAll: action.value && state.listData.data.every(i => i.isSelected || (action.value && i.id === action.dataCollectorId)),
        listData: {...state.listData, data : assignInArray(state.listData.data, i => i.id === action.dataCollectorId, item => ({ ...item, isSelected: action.value }))}
      };

    case actions.SELECT_ALL_DATA_COLLECTOR:
      return { 
        ...state, 
        listSelectedAll: action.value, 
        listData: {...state.listData, data : assignInArray(state.listData.data, _ => true, item => ({ ...item, isSelected: action.value }))}
      };

    case actions.OPEN_DATA_COLLECTOR_EDITION.INVOKE:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_DATA_COLLECTOR_EDITION.REQUEST:
      return { ...state, formFetching: true, formData: null };

    case actions.OPEN_DATA_COLLECTOR_EDITION.SUCCESS:
      return { ...state, formFetching: false, formData: action.data, filters: initialState.dataCollectors.filters, listStale: filterIsSet(state.filters) };

    case actions.OPEN_DATA_COLLECTOR_EDITION.FAILURE:
      return { ...state, formFetching: false };

    case actions.OPEN_DATA_COLLECTOR_CREATION.REQUEST:
      return { ...state, formFetching: true, nationalSocietyId: null, formDefaultSupervisorId: null, formRegions: [], formSupervisors: [], formDefaultLocation: null };

    case actions.OPEN_DATA_COLLECTOR_CREATION.SUCCESS:
      return { ...state, formFetching: false, nationalSocietyId: action.nationalSocietyId, formDefaultSupervisorId: action.defaultSupervisorId, formRegions: action.regions, formSupervisors: action.supervisors, formDefaultLocation: action.defaultLocation, filters: initialState.dataCollectors.filters, listStale: filterIsSet(state.filters) };

    case actions.OPEN_DATA_COLLECTOR_CREATION.FAILURE:
      return { ...state, formFetching: false, formError: action.message };

    case actions.GET_DATA_COLLECTORS_MAP_OVERVIEW.SUCCESS:
      return { ...state, mapOverviewFilters: action.filters, mapOverviewCenterLocation: action.centerLocation , mapOverviewDataCollectorLocations: action.dataCollectorLocations };

    case actions.GET_DATA_COLLECTORS_MAP_OVERVIEW.FAILURE:
      return { ...state, mapOverviewCenterLocation: null , mapOverviewDataCollectorLocations: [] };

      case actions.GET_DATA_COLLECTORS_MAP_DETAILS.REQUEST:
      return { ...state, mapOverviewDetails: null, mapOverviewDetailsFetching: true };

    case actions.GET_DATA_COLLECTORS_MAP_DETAILS.SUCCESS:
      return { ...state, mapOverviewDetails: action.details, mapOverviewDetailsFetching: false };

    case actions.GET_DATA_COLLECTORS_MAP_DETAILS.FAILURE:
      return { ...state, mapOverviewDetailsFetching: false };

    case actions.CREATE_DATA_COLLECTOR.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.CREATE_DATA_COLLECTOR.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.CREATE_DATA_COLLECTOR.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.EDIT_DATA_COLLECTOR.REQUEST:
      return { ...state, formSaving: true, formError: null };

    case actions.EDIT_DATA_COLLECTOR.SUCCESS:
      return { ...state, formSaving: false, listStale: true };

    case actions.EDIT_DATA_COLLECTOR.FAILURE:
      return { ...state, formSaving: false, formError: action.error };

    case actions.REMOVE_DATA_COLLECTOR.REQUEST:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, true) };

    case actions.REMOVE_DATA_COLLECTOR.SUCCESS:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined), listStale: true };

    case actions.REMOVE_DATA_COLLECTOR.FAILURE:
      return { ...state, listRemoving: setProperty(state.listRemoving, action.id, undefined) };

    case actions.SET_DATA_COLLECTORS_TRAINING_STATE.REQUEST:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((trainingState, id) => setProperty(trainingState, id, true), state.updatingDataCollector) };

    case actions.SET_DATA_COLLECTORS_TRAINING_STATE.SUCCESS:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((trainingState, id) => removeProperty(trainingState, id), state.updatingDataCollector), listStale: true };

    case actions.SET_DATA_COLLECTORS_TRAINING_STATE.FAILURE:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((trainingState, id) => removeProperty(trainingState, id), state.updatingDataCollector) };

    case actions.OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.SUCCESS:
      return { ...state, filtersData: action.filtersData };

    case actions.GET_DATA_COLLECTORS_PERFORMANCE.REQUEST:
      return { ...state, performanceListFetching: true, performanceListData: { data: [], page: null, rowsPerPage: null, totalRows: null } };

    case actions.GET_DATA_COLLECTORS_PERFORMANCE.SUCCESS:
      return { ...state, performanceListFetching: false, performanceListData: action.list, completeness: action.completeness, performanceListFilters: action.filters };

    case actions.GET_DATA_COLLECTORS_PERFORMANCE.FAILURE:
      return { ...state, performanceListFetching: false, performanceListData: [] };

    case actions.REPLACE_SUPERVISOR.REQUEST:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((replacingState, id) => setProperty(replacingState, id, true), state.updatingDataCollector) };

    case actions.REPLACE_SUPERVISOR.SUCCESS:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((replacingState, id) => removeProperty(replacingState, id), state.updatingDataCollector), listStale: true };

    case actions.REPLACE_SUPERVISOR.FAILURE:
      return { ...state, updatingDataCollector: action.dataCollectorIds.reduce((replacingState, id) => removeProperty(replacingState, id), state.updatingDataCollector) };

    default:
      return state;
  }
};
