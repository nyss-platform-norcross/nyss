import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./projectOrganizationsConstants";
import * as actions from "./projectOrganizationsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";

export const projectOrganizationsSagas = () => [
  takeEvery(
    consts.OPEN_PROJECT_ORGANIZATIONS_LIST.INVOKE,
    openProjectOrganizationsList,
  ),
  takeEvery(
    consts.OPEN_PROJECT_ORGANIZATION_CREATION.INVOKE,
    openProjectOrganizationCreation,
  ),
  takeEvery(
    consts.CREATE_PROJECT_ORGANIZATION.INVOKE,
    createProjectOrganization,
  ),
  takeEvery(
    consts.REMOVE_PROJECT_ORGANIZATION.INVOKE,
    removeProjectOrganization,
  ),
];

function* openProjectOrganizationsList({ projectId }) {
  yield put(actions.openList.request());
  try {
    yield openProjectOrganizationsModule(projectId);

    if (yield select((state) => state.projectOrganizations.listStale)) {
      yield call(getProjectOrganizations, projectId);
    }

    yield put(actions.openList.success(projectId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* openProjectOrganizationCreation({ projectId }) {
  yield put(actions.openCreation.request());
  try {
    const createData = yield call(
      http.get,
      `/api/projectOrganization/getCreateData?projectId=${projectId}`,
    );
    yield openProjectOrganizationsModule(projectId);
    yield put(actions.openCreation.success(createData.value));
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
}

function* createProjectOrganization({ projectId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      `/api/projectOrganization/create?projectId=${projectId}`,
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(projectId));
    yield put(
      appActions.showMessage(stringKeys.projectOrganization.create.success),
    );
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* removeProjectOrganization({ projectId, projectOrganizationId }) {
  yield put(actions.remove.request(projectOrganizationId));
  try {
    yield call(
      http.post,
      `/api/projectOrganization/${projectOrganizationId}/delete`,
    );
    yield put(actions.remove.success(projectOrganizationId));
    yield call(getProjectOrganizations, projectId);
    yield put(
      appActions.showMessage(stringKeys.projectOrganization.delete.success),
    );
  } catch (error) {
    yield put(actions.remove.failure(projectOrganizationId, error.message));
  }
}

function* getProjectOrganizations(projectId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(
      http.get,
      `/api/projectOrganization/list?projectId=${projectId}`,
    );
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openProjectOrganizationsModule(projectId) {
  const project = yield call(http.getCached, {
    path: `/api/project/${projectId}/basicData`,
    dependencies: [entityTypes.project(projectId)],
  });

  yield put(
    appActions.openModule.invoke(null, {
      nationalSocietyId: project.value.nationalSociety.id,
      nationalSocietyName: project.value.nationalSociety.name,
      nationalSocietyCountry: project.value.nationalSociety.countryName,
      projectId: project.value.id,
      projectName: project.value.name,
      projectIsClosed: project.value.isClosed,
      allowMultipleOrganizations: project.value.allowMultipleOrganizations,
    }),
  );
}
