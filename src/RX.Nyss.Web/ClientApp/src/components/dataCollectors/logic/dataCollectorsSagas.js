import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./dataCollectorsConstants";
import * as actions from "./dataCollectorsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { strings, stringKeys } from "../../../strings";

export const dataCollectorsSagas = () => [
  takeEvery(consts.OPEN_DATA_COLLECTORS_LIST.INVOKE, openDataCollectorsList),
  takeEvery(consts.OPEN_DATA_COLLECTOR_CREATION.INVOKE, openDataCollectorCreation),
  takeEvery(consts.OPEN_DATA_COLLECTOR_EDITION.INVOKE, openDataCollectorEdition),
  takeEvery(consts.OPEN_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, openDataCollectorMapOverview),
  takeEvery(consts.GET_DATA_COLLECTORS_MAP_OVERVIEW.INVOKE, getDataCollectorMapOverview),
  takeEvery(consts.CREATE_DATA_COLLECTOR.INVOKE, createDataCollector),
  takeEvery(consts.EDIT_DATA_COLLECTOR.INVOKE, editDataCollector),
  takeEvery(consts.REMOVE_DATA_COLLECTOR.INVOKE, removeDataCollector)
];

function* openDataCollectorsList({ projectId }) {
  const listStale = yield select(state => state.dataCollectors.listStale);
  const listProjectId = yield select(state => state.dataCollectors.listProjectId);

  yield put(actions.openList.request());
  try {
    yield openDataCollectorsModule(projectId);

    if (listStale || listProjectId !== projectId) {
      yield call(getDataCollectors, projectId);
    }

    yield put(actions.openList.success(projectId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* openDataCollectorCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    yield openDataCollectorsModule(projectId);

    const response = yield call(http.get, `/api/project/${projectId}/dataCollector/formData`);

    yield put(actions.openCreation.success(response.value.regions, response.value.supervisors, response.value.defaultLocation, response.value.defaultSupervisorId));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
};

function* openDataCollectorMapOverview({ projectId, from, to }) {
  yield put(actions.openMapOverview.request());
  try {
    yield openDataCollectorsModule(projectId);
   
    yield call(getDataCollectorMapOverview, { projectId, from, to });
   
    yield put(actions.openMapOverview.success());
  } catch (error) {
    yield put(actions.openMapOverview.failure(error.message));
  }
};

function* getDataCollectorMapOverview({ projectId, from, to }) {
  yield put(actions.getMapOverview.request());
  try {
    const response = yield call(http.get, `/api/project/${projectId}/dataCollector/mapOverview?from=${from}&to=${to}`);
    yield put(actions.getMapOverview.success(response.value.dataCollectorLocations, response.value.centerLocation));
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
    const response = yield call(http.post, `/api/project/${projectId}/dataCollector/create`, data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(appActions.showMessage(strings(stringKeys.nationalSocietyUser.messages.creationSuccessful)));
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
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeDataCollector({ dataCollectorId }) {
  yield put(actions.remove.request(dataCollectorId));
  try {
    yield call(http.post, `/api/dataCollector/${dataCollectorId}/remove`);
    yield put(actions.remove.success(dataCollectorId));
    const projectId = yield select(state => state.appData.route.params.projectId);
    yield call(getDataCollectors, projectId);
  } catch (error) {
    yield put(actions.remove.failure(dataCollectorId, error.message));
  }
};

function* getDataCollectors(projectId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/project/${projectId}/dataCollector/list`);
    yield put(actions.getList.success(response.value));
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
    projectName: project.value.name
  }));
}
