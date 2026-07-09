import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./suspectedDiseaseConstants";
import * as actions from "./suspectedDiseaseActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { stringKeys } from "../../../strings";

export const suspectedDiseaseSagas = () => [
  takeEvery(consts.GET_SUSPECTED_DISEASE.INVOKE, getSuspectedDisease),
  takeEvery(
    consts.OPEN_EDITION_SUSPECTED_DISEASE.INVOKE,
    openSuspectedDiseaseEdition,
  ),
  takeEvery(consts.EDIT_SUSPECTED_DISEASE.INVOKE, editSuspectedDisease),
  takeEvery(consts.CREATE_SUSPECTED_DISEASE.INVOKE, createSuspectedDisease),
  takeEvery(consts.REMOVE_SUSPECTED_DISEASE.INVOKE, removeSuspectedDisease),
];

function* getSuspectedDisease(force) {
  const currentData = yield select((state) => state.suspectedDiseases.listData);

  if (!force && currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, "/api/suspecteddisease/list");

    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

function* openSuspectedDiseaseEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/suspecteddisease/${params.suspecteddiseaseId}/get`,
    );

    yield put(
      appActions.openModule.invoke(path, {
        suspectedDiseaseCode: response.value.suspectedDiseaseCode,
      }),
    );

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createSuspectedDisease({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(
      http.post,
      "/api/suspecteddisease/create",
      data,
    );
    yield put(actions.create.success(response.value));
    yield put(actions.goToList());
    yield put(
      appActions.showMessage(stringKeys.suspectedDisease.create.success),
    );
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editSuspectedDisease({ id, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(
      http.post,
      `/api/suspecteddisease/${id}/edit`,
      data,
    );
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList());
    yield put(appActions.showMessage(stringKeys.suspectedDisease.edit.success));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* removeSuspectedDisease({ id }) {
  yield put(actions.remove.request(id));
  try {
    yield call(http.post, `/api/suspecteddisease/${id}/delete`);
    yield put(actions.remove.success(id));
    yield call(getSuspectedDisease, true);
    yield put(
      appActions.showMessage(stringKeys.suspectedDisease.delete.success),
    );
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
}
