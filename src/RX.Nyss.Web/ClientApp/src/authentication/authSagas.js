import { call, put, takeEvery, takeLatest, select } from "redux-saga/effects";
import * as consts from "../authentication/authConstants";
import * as appConsts from "../components/app/logic/appConstans";
import * as authActions from "../authentication/authActions";
import * as appActions from "../components/app/logic/appActions";
import * as http from "../utils/http";
import * as auth from "./auth";
import { stringKeys, stringKey } from "../strings";
import { reloadPage } from "../utils/page";
import * as localStorage from "../utils/localStorage";

export const authSagas = () => [
  takeEvery(consts.LOGIN.INVOKE, login),
  takeEvery(consts.LOGOUT.INVOKE, logout),
  takeEvery(consts.VERIFY_EMAIL.INVOKE, verifyEmail),
  takeEvery(consts.RESET_PASSWORD.INVOKE, resetPassword),
  takeEvery(consts.RESET_PASSWORD_CALLBACK.INVOKE, resetPasswordCallback),
  takeLatest(appConsts.PAGE_FOCUSED, pageFocused),
];

function* login({ userName, password, redirectUrl }) {
  yield put(authActions.login.request());
  try {
    const data = yield call(
      http.post,
      "/api/authentication/login",
      { userName, password },
      true,
    );

    if (!data.isSuccess) {
      throw new Error(stringKey(data.message.key));
    }

    yield put(authActions.login.success());

    const agreementStatusResponse = yield call(
      http.get,
      "/api/agreement/listPending",
    );

    if (
      agreementStatusResponse.value.pendingSocieties.length > 0 ||
      agreementStatusResponse.value.staleSocieties.length > 0
    ) {
      auth.redirectTo("/agreements");
    } else {
      auth.redirectToRoot();
    }
  } catch (error) {
    yield put(authActions.login.failure(error.message));
  }
}

function* logout() {
  yield put(authActions.logout.request());
  try {
    yield call(http.post, "/api/authentication/logout");
    localStorage.remove(consts.localStorageUserIdKey);
    yield put(authActions.logout.success());
    auth.redirectToLogin();
  } catch (error) {
    yield put(authActions.logout.failure(error.message));
  }
}

function* verifyEmail({ password, email, token }) {
  yield put(authActions.verifyEmail.request());
  try {
    yield call(
      http.post,
      "/api/userverification/verifyEmailAndAddPassword",
      { password, email, token },
      true,
    );
    yield put(authActions.verifyEmail.success());
    yield put(authActions.login.invoke(email, password));
  } catch (error) {
    yield put(authActions.verifyEmail.failure(error.message));
  }
}

function* resetPassword({ email }) {
  yield put(authActions.resetPassword.request());
  try {
    yield call(
      http.post,
      "/api/userverification/resetPassword",
      { email },
      true,
    );
    yield put(authActions.resetPassword.success());
    yield put(appActions.showMessage(stringKeys.user.resetPassword.emailSent));
  } catch (error) {
    yield put(authActions.resetPassword.failure(error.message));
  }
}

function* resetPasswordCallback({ password, email, token }) {
  yield put(authActions.resetPasswordCallback.request());
  try {
    yield call(
      http.post,
      "/api/userverification/resetPasswordCallback",
      { password, email, token },
      true,
    );
    yield put(authActions.resetPasswordCallback.success());
    yield put(authActions.login.invoke(email, password));
  } catch (error) {
    yield put(authActions.resetPasswordCallback.failure(error.message));
  }
}

function* pageFocused() {
  const user = yield select((state) => state.appData.user);
  const userId = user ? user.id.toString() : null;
  const storageUserId = localStorage.get(consts.localStorageUserIdKey) || null;

  if (storageUserId !== userId) {
    reloadPage();
  }
}
