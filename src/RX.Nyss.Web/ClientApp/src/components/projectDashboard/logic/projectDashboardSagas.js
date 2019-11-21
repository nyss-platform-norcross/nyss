import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./projectDashboardConstants";
import * as actions from "./projectDashboardActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";

export const projectDashboardSagas = () => [
  takeEvery(consts.OPEN_PROJECT_DASHBOARD.INVOKE, openProjectDashboard)
];

function* openProjectDashboard({ projectId }) {
  yield put(actions.openDashbaord.request());
  try {
    const project = yield call(openProjectDashboardModule, projectId);
    const projectSummary = yield call(http.get, `/api/project/${projectId}/summary`);
    yield put(actions.openDashbaord.success(project.name, projectSummary.value));
  } catch (error) {
    yield put(actions.openDashbaord.failure(error.message));
  }
};

function* openProjectDashboardModule(projectId) {
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

  return project.value;
}
