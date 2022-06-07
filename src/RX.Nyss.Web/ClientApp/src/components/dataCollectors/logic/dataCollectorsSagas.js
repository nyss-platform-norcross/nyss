import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./dataCollectorsConstants";
import * as actions from "./dataCollectorsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";
import dayjs from "dayjs";
import { downloadFile } from "../../../utils/downloadFile";
import { convertToUtc, formatDate } from "../../../utils/date";

export const dataCollectorsSagas = () => [
  takeEvery(consts.OPEN_DATA_COLLECTORS_LIST.INVOKE, openDataCollectorsList),
  takeEvery(consts.OPEN_DATA_COLLECTOR_CREATION.INVOKE, openDataCollectorCreation),
  takeEvery(consts.OPEN_DATA_COLLECTOR_EDITION.INVOKE, openDataCollectorEdition),
  takeEvery(consts.OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, openDataCollectorMapOverview),
  takeEvery(consts.GET_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, getDataCollectorMapOverview),
  takeEvery(consts.CREATE_DATA_COLLECTOR.INVOKE, createDataCollector),
  takeEvery(consts.EDIT_DATA_COLLECTOR.INVOKE, editDataCollector),
  takeEvery(consts.REMOVE_DATA_COLLECTOR.INVOKE, removeDataCollector),
  takeEvery(consts.GET_DATA_COLLECTORS_MAP_DETAILS.INVOKE, getMapDetails),
  takeEvery(consts.SET_DATA_COLLECTORS_TRAINING_STATE.INVOKE, setTrainingState),
  takeEvery(consts.OPEN_DATA_COLLECTORS_PERFORMANCE_LIST.INVOKE, openDataCollectorsPerformanceList),
  takeEvery(consts.GET_DATA_COLLECTORS_PERFORMANCE.INVOKE, getDataCollectorsPerformance),
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_EXCEL.INVOKE, getExcelExportData),
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_CSV.INVOKE, getCsvExportData),
  takeEvery(consts.GET_DATA_COLLECTORS.INVOKE, getDataCollectors),
  takeEvery(consts.REPLACE_SUPERVISOR.INVOKE, replaceSupervisor),
  takeEvery(consts.SET_DATA_COLLECTORS_DEPLOYED_STATE.INVOKE, setDeployedState),
  takeEvery(consts.EXPORT_DATA_COLLECTOR_PERFORMANCE.INVOKE, exportDataCollectorPerformance)
];

function* openDataCollectorsList({ projectId }) {
  const listStale = yield select(state => state.dataCollectors.listStale);
  const listProjectId = yield select(state => state.dataCollectors.projectId);
  const filtersStale = yield select(state => state.dataCollectors.filtersStale);

  yield put(actions.openList.request());
  try {
    yield openDataCollectorsModule(projectId);

    const filtersData = listProjectId !== projectId || filtersStale
      ? (yield call(http.get, `/api/dataCollector/filters?projectId=${projectId}`)).value
      : yield select(state => state.dataCollectors.filtersData);

    const filters = yield select(state => state.dataCollectors.filters);

    if (listStale || listProjectId !== projectId) {
      yield call(getDataCollectors, { projectId, filters });
    }

    yield put(actions.openList.success(projectId, filtersData));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* openDataCollectorCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    yield openDataCollectorsModule(projectId);

    const response = yield call(http.get, `/api/dataCollector/formData?projectId=${projectId}`);

    yield put(actions.openCreation.success(response.value.regions, response.value.supervisors, response.value.defaultLocation, response.value.defaultSupervisorId, response.value.countryCode));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
};

function* openDataCollectorMapOverview({ projectId }) {
  yield put(actions.openMapOverview.request());
  try {
    yield openDataCollectorsModule(projectId);

    let endDate = convertToUtc(dayjs(new Date()));
    endDate = endDate.set('hour', 0);
    endDate = endDate.set('minute', 0);
    endDate = endDate.set('second', 0);
    const filters = (yield select(state => state.dataCollectors.mapOverviewFilters)) ||
    {
      startDate: endDate.add(-7, 'day'),
      endDate: endDate
    };

    yield call(getDataCollectorMapOverview, { projectId, filters })
    yield put(actions.openMapOverview.success());
  } catch (error) {
    yield put(actions.openMapOverview.failure(error.message));
  }
};

function* getDataCollectorMapOverview({ projectId, filters }) {
  yield put(actions.getMapOverview.request());
  try {
    const response = yield call(http.get, `/api/dataCollector/mapOverview?projectId=${projectId}&from=${filters.startDate}&to=${filters.endDate}`);
    yield put(actions.getMapOverview.success(filters, response.value.dataCollectorLocations, response.value.centerLocation));
  } catch (error) {
    yield put(actions.getMapOverview.failure(error.message));
  }
};

function* openDataCollectorEdition({ dataCollectorId }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(http.get, `/api/dataCollector/${dataCollectorId}/get`);
    yield openDataCollectorsModule(response.value.projectId);
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* createDataCollector({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/create`, data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(data.projectId));
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.create.failure(error));
  }
};

function* editDataCollector({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/${data.id}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(data.projectId));
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
};

function* removeDataCollector({ dataCollectorId }) {
  yield put(actions.remove.request(dataCollectorId));
  try {
    const response = yield call(http.post, `/api/dataCollector/${dataCollectorId}/delete`);
    yield put(actions.remove.success(dataCollectorId));
    const projectId = yield select(state => state.appData.route.params.projectId);
    const filters = yield select(state => state.dataCollectors.filters);
    yield call(getDataCollectors, { projectId, filters });
    yield put(appActions.showMessage(response.value));
  } catch (error) {
    yield put(actions.remove.failure(dataCollectorId, error.message));
  }
};

function* getMapDetails({ projectId, lat, lng }) {
  yield put(actions.getMapDetails.request());
  try {
    const filters = yield select(state => state.dataCollectors.mapOverviewFilters);
    const response = yield call(http.get, `/api/dataCollector/mapOverviewDetails?projectId=${projectId}&from=${filters.startDate}&to=${filters.endDate}&lat=${lat}&lng=${lng}`);
    yield put(actions.getMapDetails.success(response.value));
  } catch (error) {
    yield put(actions.getMapDetails.failure(error.message));
  }
};

function* getDataCollectors({ projectId, filters }) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/list?projectId=${projectId}`, filters);
    yield put(actions.getList.success(response.value, filters));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openDataCollectorsModule(projectId) {
  const project = yield call(http.getCached, {
    path: `/api/project/${projectId}/basicData`,
    dependencies: [entityTypes.project(projectId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: project.value.nationalSociety.id,
    nationalSocietyName: project.value.nationalSociety.name,
    nationalSocietyCountry: project.value.nationalSociety.countryName,
    projectId: project.value.id,
    projectName: project.value.name,
    projectIsClosed: project.value.isClosed
  }));
};

function* setTrainingState({ dataCollectorIds, inTraining }) {
  yield put(actions.setTrainingState.request(dataCollectorIds));
  try {
    const response = yield call(http.post, "/api/dataCollector/setTrainingState", { dataCollectorIds, inTraining });
    yield put(actions.setTrainingState.success(dataCollectorIds, inTraining));
    yield put(appActions.showMessage(response.message.key));
    const projectId = yield select(state => state.dataCollectors.projectId);
    const filters = yield select(state => state.dataCollectors.filters);
    yield call(getDataCollectors, { projectId, filters });
  } catch (error) {
    yield put(actions.setTrainingState.failure(dataCollectorIds, error.message));
  }
};

function* openDataCollectorsPerformanceList({ projectId }) {
  yield put(actions.openDataCollectorsPerformanceList.request());
  try {
    yield openDataCollectorsModule(projectId);

    const filters = yield select(state => state.dataCollectors.performanceListFilters);

    let filtersData = yield select(state => state.dataCollectors.filtersData);

    if (filtersData.nationalSocietyId == null) {
      const fetchedFiltersData = yield call(http.get, `/api/dataCollector/filters?projectId=${projectId}`);
      filtersData = fetchedFiltersData.value;
    }

    yield call(getDataCollectorsPerformance, { projectId, filters });
    yield put(actions.openDataCollectorsPerformanceList.success(filters, filtersData));
  } catch (error) {
    yield put(actions.openDataCollectorsPerformanceList.failure(error.message));
  }
};

function* getDataCollectorsPerformance({ projectId, filters }) {
  yield put(actions.getDataCollectorsPerformanceList.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/performance?projectId=${projectId}`, filters);
    yield put(actions.getDataCollectorsPerformanceList.success(response.value.performance, response.value.completeness, response.value.epiDateRange));
  } catch (error) {
    yield put(actions.getDataCollectorsPerformanceList.failure(error.message));
  }
};

function* getExcelExportData({ projectId, filters }) {
  yield put(actions.exportToExcel.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/dataCollector/exportToExcel?projectId=${projectId}`,
      fileName: `dataCollectors_${formatDate(date)}.xlsx`,
      data: filters
    });

    yield put(actions.exportToExcel.success());
  } catch (error) {
    yield put(actions.exportToExcel.failure(error.message));
  }
};

function* getCsvExportData({ projectId, filters }) {
  yield put(actions.exportToCsv.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/dataCollector/exportToCsv?projectId=${projectId}`,
      fileName: `dataCollectors_${formatDate(date)}.csv`,
      data: filters
    });

    yield put(actions.exportToCsv.success());
  } catch (error) {
    yield put(actions.exportToCsv.failure(error.message));
  }
};

function* replaceSupervisor({ dataCollectorIds, supervisorId, supervisorRole }) {
  yield put(actions.replaceSupervisor.request(dataCollectorIds));
  try {
    const projectId = yield select(state => state.dataCollectors.projectId);
    const filters = yield select(state => state.dataCollectors.filters);
    yield call(http.post, '/api/dataCollector/replaceSupervisor', { dataCollectorIds, supervisorId, supervisorRole });
    yield call(getDataCollectors, { projectId, filters });
    yield put(actions.replaceSupervisor.success(dataCollectorIds));
    yield put(appActions.showMessage(stringKeys.dataCollectors.list.supervisorReplacedSuccessfully));
  } catch (error) {
    yield put(actions.replaceSupervisor.failure(dataCollectorIds, error));
  }
};

function* setDeployedState({ dataCollectorIds, deployed }) {
  yield put(actions.setDeployedState.request(dataCollectorIds));
  try {
    const projectId = yield select(state => state.dataCollectors.projectId);
    const filters = yield select(state => state.dataCollectors.filters);
    const response = yield call(http.post, '/api/dataCollector/setDeployedState', { dataCollectorIds, deployed });
    yield call(getDataCollectors, { projectId, filters });
    yield put(actions.setDeployedState.success(dataCollectorIds));
    yield put(appActions.showMessage(response.message.key));
  } catch (error) {
    yield put(actions.setDeployedState.failure(dataCollectorIds, error));
  }
};

function* exportDataCollectorPerformance({ projectId, filters }) {
  yield put(actions.exportDataCollectorPerformance.request());
  try {
    const date = new Date(Date.now());
    yield downloadFile({
      url: `/api/dataCollector/exportPerformance?projectId=${projectId}`,
      fileName: `dataCollectorPerformance_${formatDate(date)}.xlsx`,
      data: filters
    });

    yield put(actions.exportDataCollectorPerformance.success());
  } catch (error) {
    yield put(actions.exportDataCollectorPerformance.failure(error.message));
  }
}

