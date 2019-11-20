import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./reportsConstants";
import * as actions from "./reportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { strings, stringKeys } from "../../../strings";

export const reportsSagas = () => [
  takeEvery(consts.OPEN_REPORTS_LIST.INVOKE, openReportsList),
];

function* openReportsList({ projectId }) {
  yield put(actions.openList.request());
  try {
    yield openReportsModule(projectId);

    if (yield select(state => state.reports.listStale)) {
      yield call(getReports, projectId);
    }

    yield put(actions.openList.success());
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getReports(projectId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/project/${projectId}/report/list?pageNumber=1`);
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* openReportsModule(projectId) {
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
