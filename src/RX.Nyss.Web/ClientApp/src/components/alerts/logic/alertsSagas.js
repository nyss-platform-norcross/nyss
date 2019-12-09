import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./alertsConstants";
import * as actions from "./alertsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";

export const alertsSagas = () => [
  takeEvery(consts.OPEN_ALERTS_LIST.INVOKE, openAlertsList),
  takeEvery(consts.GET_ALERTS.INVOKE, getAlerts)
];

function* openAlertsList({ projectId }) {
  const listStale = yield select(state => state.alerts.listStale);

  yield put(actions.openList.request());
  try {
    yield openAlertsModule(projectId);

    if (listStale) {
      yield call(getAlerts, { projectId });
    }

    yield put(actions.openList.success(projectId));

  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getAlerts({ projectId, pageNumber }) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/alert/list?projectId=${projectId}&pageNumber=${pageNumber || 1}`);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openAlertsModule(projectId) {
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
