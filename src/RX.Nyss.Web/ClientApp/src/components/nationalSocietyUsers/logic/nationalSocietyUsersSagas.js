import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyUsersConstants";
import * as actions from "./nationalSocietyUsersActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import * as roles from "../../../authentication/roles";

export const nationalSocietyUsersSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USERS_LIST.INVOKE, openNationalSocietyUsersList),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USER_CREATION.INVOKE, openNationalSocietyUserCreation),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USER_EDITION.INVOKE, openNationalSocietyUserEdition),
  takeEvery(consts.CREATE_NATIONAL_SOCIETY_USER.INVOKE, createNationalSocietyUser),
  takeEvery(consts.EDIT_NATIONAL_SOCIETY_USER.INVOKE, editNationalSocietyUser),
  takeEvery(consts.REMOVE_NATIONAL_SOCIETY_USER.INVOKE, removeNationalSocietyUser)
];

function* openNationalSocietyUsersList({ nationalSocietyId }) {
  yield put(actions.openList.request());
  try {
    yield openNationalSocietyUsersModule(nationalSocietyId);

    if (yield select(state => state.nationalSocietyUsers.listStale)) {
      yield call(getNationalSocietyUsers, nationalSocietyId);
    }

    yield put(actions.openList.success());
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* openNationalSocietyUserCreation({ nationalSocietyId }) {
  yield put(actions.openCreation.request());
  try {
    yield openNationalSocietyUsersModule(nationalSocietyId);
    yield put(actions.openCreation.success());
  } catch (error) {
    yield put(actions.openCreation.failure(error.message));
  }
};

function* openNationalSocietyUserEdition({ nationalSocietyUserId }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(http.get, `/api/nationalSocietyUser/${nationalSocietyUserId}/get`);
    yield openNationalSocietyUsersModule(response.value.nationalSocietyId);
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* createNationalSocietyUser({ nationalSocietyId, data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, getSpecificRoleUserAdditionUrl(nationalSocietyId, data.role), data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
    yield put(appActions.showMessage("The User was added successfully"));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editNationalSocietyUser({ nationalSocietyId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/nationalSocietyUser/${data.id}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList(nationalSocietyId));
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeNationalSocietyUser({ nationalSocietyUserId, role }) {
  yield put(actions.remove.request(nationalSocietyUserId));
  try {
    yield call(http.post, getSpecificRoleUserRemovalUrl(nationalSocietyUserId, role));
    yield put(actions.remove.success(nationalSocietyUserId));
  } catch (error) {
    yield put(actions.remove.failure(nationalSocietyUserId, error.message));
  }
};

function* getNationalSocietyUsers(nationalSocietyId) {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/nationalSociety/${nationalSocietyId}/user/list`);
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function getSpecificRoleUserRemovalUrl(userId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
        return `/api/nationalSociety/technicalAdvisor/${userId}/remove`;
    case roles.DataManager:
        return `/api/nationalSociety/dataManager/${userId}/remove`;
    case roles.DataConsumer:
        return `/api/nationalSociety/dataConsumer/${userId}/remove`;
  }
};

function getSpecificRoleUserAdditionUrl(nationalSocietyId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
        return `/api/nationalSociety/${nationalSocietyId}/technicalAdvisor/create`;
    case roles.DataManager:
        return `/api/nationalSociety/${nationalSocietyId}/dataManager/create`;
    case roles.DataConsumer:
        return `/api/nationalSociety/${nationalSocietyId}/dataConsumer/create`;
  }
};

function* openNationalSocietyUsersModule(nationalSocietyId) {
  const nationalSociety = yield call(http.getCached, {
    path: `/api/nationalSociety/${nationalSocietyId}/get`,
    dependencies: [entityTypes.nationalSociety(nationalSocietyId)]
  });

  yield put(appActions.openModule.invoke(null, {
    nationalSocietyId: nationalSociety.value.id,
    nationalSocietyName: nationalSociety.value.name,
    nationalSocietyCountry: nationalSociety.value.countryName
  }));
}