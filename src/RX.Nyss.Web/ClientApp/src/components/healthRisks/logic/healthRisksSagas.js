import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./healthRisksConstants";
import * as actions from "./healthRisksActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";
import { stringKeys } from "../../../strings";

export const healthRisksSagas = () => [
  takeEvery(consts.GET_HEALTH_RISKS.INVOKE, getHealthRisks),
  takeEvery(consts.OPEN_EDITION_HEALTH_RISK.INVOKE, openHealthRiskEdition),
  takeEvery(consts.OPEN_CREATION_HEALTH_RISK.INVOKE, openHealthRiskCreation),
  takeEvery(consts.EDIT_HEALTH_RISK.INVOKE, editHealthRisk),
  takeEvery(consts.CREATE_HEALTH_RISK.INVOKE, createHealthRisk),
  takeEvery(consts.REMOVE_HEALTH_RISK.INVOKE, removeHealthRisk),
];

function* getHealthRisks(force) {
  const currentData = yield select((state) => state.healthRisks.listData);
  if (!force && currentData.length) {
    return;
  }

  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, "/api/healthrisk/list");
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
}

//Added to handle the list of suspected diseases in helth risk creation
function* openHealthRiskCreation() {
  yield put(actions.openCreation.request());
  try {
    const formData = yield call(http.get, "/api/suspecteddisease/list");
    yield put(actions.openCreation.success(formData.value));
  } catch (error) {
    yield put(actions.openCreation.failure(error));
  }
}

//Changed to handle the list of suspected diseases in helth risk edition
function* openHealthRiskEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(
      http.get,
      `/api/healthrisk/${params.healthRiskId}/get`,
    );
    const formData = yield call(http.get, "/api/suspecteddisease/list");
    yield put(
      appActions.openModule.invoke(path, {
        healthRiskCode: response.value.healthRiskCode,
      }),
    );
    response.value.suspectedDiseasesList = formData.value;
    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
}

function* createHealthRisk({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, "/api/healthrisk/create", data);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList());
    yield put(appActions.showMessage(stringKeys.healthRisk.create.success));
  } catch (error) {
    yield put(actions.create.failure(error));
  }
}

function* editHealthRisk({ id, data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/healthrisk/${id}/edit`, data);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList());
    yield put(appActions.showMessage(stringKeys.healthRisk.edit.success));
  } catch (error) {
    yield put(actions.edit.failure(error));
  }
}

function* removeHealthRisk({ id }) {
  yield put(actions.remove.request(id));
  try {
    yield call(http.post, `/api/healthrisk/${id}/delete`);
    yield put(actions.remove.success(id));
    yield call(getHealthRisks, true);
    yield put(appActions.showMessage(stringKeys.healthRisk.delete.success));
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
}
