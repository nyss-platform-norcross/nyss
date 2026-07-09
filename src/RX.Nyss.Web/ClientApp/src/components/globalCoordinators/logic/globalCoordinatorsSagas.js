import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./globalCoordinatorsConstants";
import * as actions from "./globalCoordinatorsActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { stringKeys } from "../../../strings";

export const globalCoordinatorsSagas = () => [
  takeEvery(consts.GET_GLOBAL_COORDINATORS.INVOKE, getGlobalCoordinators),
  takeEvery(
    consts.OPEN_EDITION_GLOBAL_COORDINATOR.INVOKE,
    openGlobalCoordinatorEdition,
  ),
  takeEvery(consts.EDIT_GLOBAL_COORDINATOR.INVOKE, editGlobalCoordinator),
  takeEvery(consts.CREATE_GLOBAL_COORDINATOR.INVOKE, createGlobalCoordinator),
  takeEvery(consts.REMOVE_GLOBAL_COORDINATOR.INVOKE, removeGlobalCoordinator),
];

function* getGlobalCoordinators(force) {
  const currentData = yield select(
    (state) => state.globalCoordinators.listData,
  );

  if (!force && currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, "/api/globalcoordinator/list");

    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openGlobalCoordinatorEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/globalCoordinator/${params.globalCoordinatorId}/get`,
    );

    yield put(
      appActions.openModule.invoke(path, {
        globalCoordinatorName: response.value.name,
        globalCoordinatorId: response.value.id,
      }),
    );

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createGlobalCoordinator({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      "/api/globalcoordinator/create",
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList());
    yield put(
      appActions.showMessage(stringKeys.globalCoordinator.create.success),
    );
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
}

function* editGlobalCoordinator({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/globalcoordinator/${data.id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList());
    yield put(
      appActions.showMessage(stringKeys.globalCoordinator.edit.success),
    );
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
}

function* removeGlobalCoordinator({ id }) {
  yield put(actions.remove.request(id));
  try {
    yield call(http.post, `/api/globalcoordinator/${id}/delete`);
    yield put(actions.remove.success(id));
    yield call(getGlobalCoordinators, true);
    yield put(
      appActions.showMessage(stringKeys.globalCoordinator.delete.success),
    );
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
}
