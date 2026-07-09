import { call, put, takeEvery } from "redux-saga/effects";
import * as appActions from "../../app/logic/appActions";
import * as consts from "./agreementsConstants";
import * as actions from "./agreementsActions";
import * as http from "../../../utils/http";
import * as auth from "../../../authentication/auth";

export const agreementsSagas = () => [
  takeEvery(consts.OPEN_AGREEMENT_PAGE.INVOKE, getPendingAgreementDocuments),
  takeEvery(consts.ACCEPT_AGREEMENT.INVOKE, acceptAgreement),
];

function* getPendingAgreementDocuments() {
  yield put(actions.openAgreementPage.request());
  try {
    const response = yield call(
      http.get,
      "/api/agreement/pendingAgreementDocuments",
    );

    yield put(actions.openAgreementPage.success(response.value));
  } catch (error) {
    yield put(actions.openAgreementPage.failure(error.message));
  }
}

function* acceptAgreement({ selectedLanguage }) {
  yield put(actions.acceptAgreement.request());
  try {
    yield call(
      http.post,
      `/api/agreement/accept?languageCode=${selectedLanguage}`,
    );
    yield put(actions.acceptAgreement.success());
    auth.redirectToRoot();
  } catch (error) {
    yield put(appActions.showMessage(error.message));
    yield put(actions.acceptAgreement.failure(error.message));
  }
}
