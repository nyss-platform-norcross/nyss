import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./nationalSocietyUsersConstants";
import * as actions from "./nationalSocietyUsersActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { entityTypes } from "../../nationalSocieties/logic/nationalSocietiesConstants";
import * as roles from "../../../authentication/roles";
import { strings, stringKeys } from "../../../strings";

export const nationalSocietyUsersSagas = () => [
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USERS_LIST.INVOKE, openNationalSocietyUsersList),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USER_CREATION.INVOKE, openNationalSocietyUserCreation),
  takeEvery(consts.OPEN_NATIONAL_SOCIETY_USER_EDITION.INVOKE, openNationalSocietyUserEdition),
  takeEvery(consts.CREATE_NATIONAL_SOCIETY_USER.INVOKE, createNationalSocietyUser),
  takeEvery(consts.EDIT_NATIONAL_SOCIETY_USER.INVOKE, editNationalSocietyUser),
  takeEvery(consts.REMOVE_NATIONAL_SOCIETY_USER.INVOKE, removeNationalSocietyUser),
  takeEvery(consts.SET_AS_HEADMANAGER_NATIONAL_SOCIETY_USER.INVOKE, setAsHeadManagerNationalSociety)
];

function* openNationalSocietyUsersList({ nationalSocietyId }) {
  yield put(actions.openList.request());
  try {
    yield openNationalSocietyUsersModule(nationalSocietyId);

    const isListStale = yield select(state => state.nationalSocietyUsers.listStale);
    const lastNationalSocietyId = yield select(state => state.nationalSocietyUsers.listNationalSocietyId);

    if (isListStale || lastNationalSocietyId !== nationalSocietyId) {
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

function* openNationalSocietyUserEdition({ nationalSocietyUserId, role }) {
  yield put(actions.openEdition.request());
  try {
    const user = yield call(http.get, `/api/nationalSociety/user/${nationalSocietyUserId}/basicData`);
    const response = yield call(http.get, getSpecificRoleUserRetrievalUrl(nationalSocietyUserId, user.value.role));
    yield openNationalSocietyUsersModule(yield select(state => state.appData.route.params.nationalSocietyId));
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
    yield put(appActions.showMessage(strings(stringKeys.nationalSocietyUser.messages.creationSuccessful)));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editNationalSocietyUser({ nationalSocietyId, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, getSpecificRoleUserEditionUrl(data.id, data.role), data);
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
    yield put(actions.getList.success(nationalSocietyId, response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};

function* setAsHeadManagerNationalSociety({ nationalSocietyId, nationalSocietyUserId }) {
  yield put(actions.setAsHeadManager.request(nationalSocietyUserId));
  try {
    yield call(http.post, `/api/nationalSociety/${nationalSocietyId}/setHeadManager`, {userId: nationalSocietyUserId});
    yield put(actions.setAsHeadManager.success(nationalSocietyUserId));
    yield put(appActions.showMessage("Head manager set successfully"));
  } catch (error) {
    yield put(actions.setAsHeadManager.failure(nationalSocietyUserId, error.message));
  }
};

function getSpecificRoleUserAdditionUrl(nationalSocietyId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
      return `/api/nationalSociety/${nationalSocietyId}/technicalAdvisor/create`;
    case roles.Manager:
      return `/api/nationalSociety/${nationalSocietyId}/manager/create`;
    case roles.DataConsumer:
      return `/api/nationalSociety/${nationalSocietyId}/dataConsumer/create`;
    default:
      throw new Error("Role is not valid");
  }
};

function getSpecificRoleUserEditionUrl(userId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
      return `/api/nationalSociety/technicalAdvisor/${userId}/edit`;
    case roles.Manager:
      return `/api/nationalSociety/manager/${userId}/edit`;
    case roles.DataConsumer:
      return `/api/nationalSociety/dataConsumer/${userId}/edit`;
    default:
      throw new Error("Role is not valid");
  }
};

function getSpecificRoleUserRetrievalUrl(userId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
      return `/api/nationalSociety/technicalAdvisor/${userId}/get`;
    case roles.Manager:
      return `/api/nationalSociety/manager/${userId}/get`;
    case roles.DataConsumer:
      return `/api/nationalSociety/dataConsumer/${userId}/get`;
    default:
      throw new Error("Role is not valid");
  }
};

function getSpecificRoleUserRemovalUrl(userId, role) {
  switch (role) {
    case roles.TechnicalAdvisor:
      return `/api/nationalSociety/technicalAdvisor/${userId}/remove`;
    case roles.Manager:
      return `/api/nationalSociety/manager/${userId}/remove`;
    case roles.DataConsumer:
      return `/api/nationalSociety/dataConsumer/${userId}/remove`;
    default:
      throw new Error("Role is not valid");
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
