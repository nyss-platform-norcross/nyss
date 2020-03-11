import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./dataCollectorsConstants";
import * as actions from "./dataCollectorsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";
import dayjs from "dayjs";
import { downloadFile } from "../../../utils/downloadFile";

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
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_EXCEL.INVOKE, getExcelExportData),
  takeEvery(consts.EXPORT_DATA_COLLECTORS_TO_CSV.INVOKE, getCsvExportData)
  takeEvery(consts.GET_DATA_COLLECTORS.INVOKE, getDataCollectors)
];

function* openDataCollectorsList({ projectId }) {
  const listStale = yield select(state => state.dataCollectors.listStale);
  const listProjectId = yield select(state => state.dataCollectors.projectId);

  yield put(actions.openList.request());
  try {
    yield openDataCollectorsModule(projectId);
    const filtersData = yield call(http.get, `/api/dataCollector/filters?projectId=${projectId}`);
    const filters = (yield select(state => state.dataCollectors.filters)) ||
    {
      supervisorId: null,
      area: null,
      sex: null
    };

    if (listStale || listProjectId !== projectId) {
      yield call(getDataCollectors, { projectId, filters });
    }
    
    yield put(actions.openList.success(projectId, filtersData.value));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* openDataCollectorCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    yield openDataCollectorsModule(projectId);

    const response = yield call(http.get, `/api/dataCollector/formData?projectId=${projectId}`);

    yield put(actions.openCreation.success(response.value.regions, response.value.supervisors, response.value.defaultLocation, response.value.defaultSupervisorId));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
};

function* openDataCollectorMapOverview({ projectId }) {
  yield put(actions.openMapOverview.request());
  try {
    yield openDataCollectorsModule(projectId);

    const endDate = dayjs(new Date());
    const filters = (yield select(state => state.dataCollectors.mapOverviewFilters)) ||
      {
        startDate: endDate.add(-7, "day").format('YYYY-MM-DD'),
        endDate: endDate.format('YYYY-MM-DD'),
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

function* createDataCollector({ projectId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/create?projectId=${projectId}`, data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(appActions.showMessage(stringKeys.dataCollector.messages.creationSuccessful));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editDataCollector({ projectId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/dataCollector/${data.id}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(appActions.showMessage(stringKeys.dataCollector.messages.editionSuccessful));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeDataCollector({ dataCollectorId }) {
  yield put(actions.remove.request(dataCollectorId));
  try {
    yield call(http.post, `/api/dataCollector/${dataCollectorId}/delete`);
    yield put(actions.remove.success(dataCollectorId));
    const projectId = yield select(state => state.appData.route.params.projectId);
    yield call(getDataCollectors, projectId);
    yield put(appActions.showMessage(stringKeys.dataCollector.messages.deletionSuccessful));
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

function* setTrainingState({ dataCollectorId, inTraining }) {
  yield put(actions.setTrainingState.request(dataCollectorId));
  try {
    const response = yield call(http.post, `/api/dataCollector/${dataCollectorId}/setTrainingState?isIntraining=${inTraining}`);
    http.ensureResponseIsSuccess(response);
    yield put(actions.setTrainingState.success(dataCollectorId, inTraining));
    yield put(appActions.showMessage(response.message.key));
    const projectId = yield select(state => state.dataCollectors.listProjectId);
    yield call(getDataCollectors, projectId);
  } catch (error) {
    yield put(actions.setTrainingState.failure(dataCollectorId));
  }
};

function* openDataCollectorsPerformanceList({ projectId }) {
  yield put(actions.openDataCollectorsPerformanceList.request());
  try {
    yield openDataCollectorsModule(projectId);
    yield call(getDataCollectorsPerformance, projectId);
    yield put(actions.openDataCollectorsPerformanceList.success(projectId));
  } catch (error) {
    yield put(actions.openDataCollectorsPerformanceList.failure(error.message));
  }
};

function* getDataCollectorsPerformance(projectId) {
  yield put(actions.getDataCollectorsPerformanceList.request());
  try {
    const response = yield call(http.get, `/api/dataCollector/performance?projectId=${projectId}`);
    yield put(actions.getDataCollectorsPerformanceList.success(response.value));
  } catch (error) {
    yield put(actions.getDataCollectorsPerformanceList.failure(error.message));
  }
};

function* getExcelExportData({ projectId }) {
  yield put(actions.exportToExcel.request());
  try {
    yield downloadFile({
      url: `/api/dataCollector/exportToExcel?projectId=${projectId}`,
      fileName: `dataCollectors.xlsx`
    });

    yield put(actions.exportToExcel.success());
  } catch (error) {
    yield put(actions.exportToExcel.failure(error.message));
  }
};

function* getCsvExportData({ projectId }) {
  yield put(actions.exportToCsv.request());
  try {
    yield downloadFile({
      url: `/api/dataCollector/exportToCsv?projectId=${projectId}`,
      fileName: `dataCollectors.csv`
    });

    yield put(actions.exportToCsv.success());
  } catch (error) {
    yield put(actions.exportToCsv.failure(error.message));
  }
};

