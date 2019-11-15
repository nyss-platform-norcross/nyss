import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "./headManagerConsentsConstants";
import * as actions from "./headManagerConsentsActions";
import * as http from "../../../utils/http";

export const headManagerConsentsSagas = () => [
  takeEvery(consts.OPEN_HEAD_MANAGER_CONSENTS_PAGE.INVOKE, getPendingConsents),
  takeEvery(consts.CONSENT_AS_HEAD_MANAGER.INVOKE, consentAsHeadManager)
];

function* getPendingConsents() {
  yield put(actions.openHeadManagerConsentsPage.request());
  try {
    const response = yield call(http.get, "/api/nationalSociety/pendingConsents");

    yield put(actions.openHeadManagerConsentsPage.success(response.value));
  } catch (error) {
    yield put(actions.openHeadManagerConsentsPage.failure(error.message));
  }
}

function* consentAsHeadManager() {
  yield put(actions.consentAsHeadManager.request());
  try {
    yield call(http.post, "/api/nationalSociety/consentAsHeadManager");
    yield put(actions.consentAsHeadManager.success());
    window.location.href = "/";
  } catch (error) {
    yield put(actions.consentAsHeadManager.failure(error.message));
  }
}
