import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./organizationsConstants";
import * as actions from "./organizationsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import { stringKeys } from "../../../strings";

export const organizationsSagas = () => [
  takeEvery(consts.OPEN_ORGANIZATIONS_LIST.INVOKE, openOrganizationsList),
  takeEvery(consts.OPEN_ORGANIZATION_CREATION.INVOKE, openOrganizationCreation),
  takeEvery(consts.OPEN_ORGANIZATION_EDITION.INVOKE, openOrganizationEdition),
  takeEvery(consts.CREATE_ORGANIZATION.INVOKE, createOrganization),
  takeEvery(consts.EDIT_ORGANIZATION.INVOKE, editOrganization),
  takeEvery(consts.REMOVE_ORGANIZATION.INVOKE, removeOrganization),
];

function* openOrganizationsList({ nationalSocietyId }) {
  yield put(actions.openList.request());
  try {
    yield openOrganizationsModule(nationalSocietyId);

    if (yield select((state) => state.organizations.listStale)) {
      yield call(getOrganizations, nationalSocietyId);
    }

    yield put(actions.openList.success(nationalSocietyId));
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
}

function* openOrganizationCreation({ nationalSocietyId }) {
  yield put(actions.openCreation.request());
  try {
    yield openOrganizationsModule(nationalSocietyId);
    yield put(actions.openCreation.success());
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
}

function* openOrganizationEdition({ nationalSocietyId, organizationId }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/organization/${organizationId}/get`,
    );
    yield openOrganizationsModule(nationalSocietyId);
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createOrganization({ nationalSocietyId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      `/api/organization/create?nationalSocietyId=${nationalSocietyId}`,
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
    yield put(appActions.showMessage(stringKeys.organization.create.success));
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editOrganization({ nationalSocietyId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/organization/${data.id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
    yield put(appActions.showMessage(stringKeys.organization.edit.success));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* removeOrganization({ nationalSocietyId, organizationId }) {
  yield put(actions.remove.request(organizationId));
  try {
    yield call(http.post, `/api/organization/${organizationId}/delete`);
    yield put(actions.remove.success(organizationId));
    yield call(getOrganizations, nationalSocietyId);
    yield put(appActions.showMessage(stringKeys.organization.delete.success));
  } catch (error) {
    yield put(actions.remove.failure(organizationId, error));
  }
}

function* getOrganizations(nationalSocietyId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(
      http.get,
      `/api/organization/list?nationalSocietyId=${nationalSocietyId}`,
    );
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error));
  }
}

function* openOrganizationsModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)],
  });

  yield put(
    appActions.openModule.invoke(null, {
      nationalSocietyId: nationalSociety.value.id,
      nationalSocietyName: nationalSociety.value.name,
      nationalSocietyCountry: nationalSociety.value.countryName,
      nationalSocietyIsArchived: nationalSociety.value.isArchived,
      nationalSocietyHasCoordinator:
        nationalSociety.value.nationalSocietyHasCoordinator,
      isCurrentUserHeadManager: nationalSociety.value.isCurrentUserHeadManager,
    }),
  );
}
