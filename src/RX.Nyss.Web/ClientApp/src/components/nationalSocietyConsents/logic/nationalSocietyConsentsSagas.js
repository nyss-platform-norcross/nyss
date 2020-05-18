import { call, put, takeEvery } from "redux-saga/effects";
import * as appActions from "../../app/logic/appActions";
import * as consts from "./nationalSocietyConsentsConstants";
import * as actions from "./nationalSocietyConsentsActions";
import * as http from "../../../utils/http";

export const nationalSocietyConsentsSagas = () => [
  takeEvery(consts.OPEN_HEAD_MANAGER_CONSENTS_PAGE.INVOKE, getPendingConsents),
  takeEvery(consts.CONSENT_AS_HEAD_MANAGER.INVOKE, consentAsHeadManager)
];

function* getPendingConsents() {
  yield put(actions.openNationalSocietyConsentsPage.request());
  try {
    const response = yield call(http.get, "/api/nationalSociety/pendingConsents");

    yield put(actions.openNationalSocietyConsentsPage.success(response.value));
  } catch (error) {
    yield put(actions.openNationalSocietyConsentsPage.failure(error.message));
  }
}

function* consentAsHeadManager({ selectedLanguage }) {
  yield put(actions.consentAsHeadManager.request());
  try {
    yield call(http.post, `/api/nationalSociety/consentToAgreement?languageCode=${selectedLanguage}`);
    yield put(actions.consentAsHeadManager.success());
    window.location.href = "/";
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.consentAsHeadManager.failure(error.message));
  }
}
