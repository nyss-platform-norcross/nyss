import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./reportsConstants";
import * as actions from "./reportsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";

export const reportsSagas = () => [
  takeEvery(consts.OPEN_REPORTS_LIST.INVOKE, openReportsList),
  takeEvery(consts.GET_REPORTS.INVOKE, getReports)
];

function* openReportsList({ projectId }) {
  const listStale = yield select(state => state.reports.listStale);

  yield put(actions.openList.request());
  try {
    yield openReportsModule(projectId);

    if (listStale) {
      yield call(getReports, { projectId });
    }

    yield put(actions.openList.success(projectId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getReports({ projectId, pageNumber, reportListFilter }) {
  yield put(actions.getList.request());
  try {
    const filter = reportListFilter || {
      reportListType: "main"
    };

    const response = yield call(http.post, `/api/report/list?projectId=${projectId}&pageNumber=${pageNumber || 1}`, filter);
    http.ensureResponseIsSuccess(response);
    yield put(actions.getList.success(response.value.data, response.value.page, response.value.rowsPerPage, response.value.totalRows, filter));
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
