import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./healthRisksConstants";
import * as actions from "./healthRisksActions";
import * as appActions from "../../app/logic/appActions";
import * as http from "../../../utils/http";

export const healthRisksSagas = () => [
  takeEvery(consts.GET_HEALTH_RISKS.INVOKE, getHealthRisks),
  takeEvery(consts.OPEN_EDITION_HEALTH_RISK.INVOKE, openHealthRiskEdition),
  takeEvery(consts.EDIT_HEALTH_RISK.INVOKE, editHealthRisk),
  takeEvery(consts.CREATE_HEALTH_RISK.INVOKE, createHealthRisk),
  takeEvery(consts.REMOVE_HEALTH_RISK.INVOKE, removeHealthRisk)
];

function* getHealthRisks(force) {
  const currentData = yield select(state => state.healthRisks.listData)

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
};

function* openHealthRiskEdition({ path, params }) {
  yield put(actions.openEdition.request());
  try {
    const response = yield call(http.get, `/api/healthRisk/${params.healthRiskId}/get`);

    yield put(appActions.openModule.invoke(path, {
      healthRiskCode: response.value.healthRiskCode
    }));

    yield put(actions.openEdition.success(response.value));
  } catch (error) {
    yield put(actions.openEdition.failure(error.message));
  }
};

function* createHealthRisk({ data }) {
  yield put(actions.create.request());
  try {
    const response = yield call(http.post, "/api/healthrisk/create", data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.create.success(response.value));
    yield put(actions.goToList());
    yield put(appActions.showMessage("The Health Risk / Event was added successfully"));
  } catch (error) {
    yield put(actions.create.failure(error.message));
  }
};

function* editHealthRisk({ data }) {
  yield put(actions.edit.request());
  try {
    const response = yield call(http.post, `/api/healthrisk/${data.id}/edit`, data);
    http.ensureResponseIsSuccess(response);
    yield put(actions.edit.success(response.value));
    yield put(actions.goToList());
  } catch (error) {
    yield put(actions.edit.failure(error.message));
  }
};

function* removeHealthRisk({ id }) {
  yield put(actions.remove.request(id));
  try {
    yield call(http.post, `/api/healthrisk/${id}/remove`);
    yield put(actions.remove.success(id));
    yield call(getHealthRisks, true);
  } catch (error) {
    yield put(actions.remove.failure(id, error.message));
  }
};