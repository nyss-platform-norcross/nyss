import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "../authentication/authConstants";
import * as authActions from "../authentication/authActions";
import * as appActions from "../components/app/logic/appActions";
import * as http from "../utils/http";
import * as auth from "./auth";
import { strings } from "../strings";

export const authSagas = () => [
  takeEvery(consts.LOGIN.INVOKE, login),
  takeEvery(consts.LOGOUT.INVOKE, logout),
  takeEvery(consts.VERIFY_EMAIL.INVOKE, verifyEmail),
  takeEvery(consts.RESET_PASSWORD.INVOKE, resetPassword),
  takeEvery(consts.RESET_PASSWORD_CALLBACK.INVOKE, resetPasswordCallback)
];

function* login({ userName, password, redirectUrl }) {
  yield put(authActions.login.request());
  try {
    const data = yield call(http.post, "/api/authentication/login", { userName, password }, true);

    if (!data.isSuccess) {
      throw new Error(strings(data.message.key));
    }

    auth.setAccessToken(data.value.accessToken)
    yield put(authActions.login.success());

    if (redirectUrl) {
      auth.redirectTo(redirectUrl);
    } else {
      auth.redirectToRoot();
    }
  } catch (error) {
    yield put(authActions.login.failure(error.message));
  }
};

function* logout() {
  yield put(authActions.logout.request());
  try {
    yield call(http.post, "/api/authentication/logout");
    auth.removeAccessToken();
    yield put(authActions.logout.success());
    auth.redirectToLogin();
  } catch (error) {
    yield put(authActions.logout.failure(error.message));
  }
};

function* verifyEmail({ password, email, token }) {
  yield put(authActions.verifyEmail.request());
  try {
    yield call(http.post, "/api/userverification/verifyEmailAndAddPassword", { password, email, token }, true);
    yield put(authActions.verifyEmail.success());
    yield put(authActions.login.invoke(email, password));
  } catch (error) {
    yield put(authActions.verifyEmail.failure(error.message));
  }
};

function* resetPassword({ email }) {
  yield put(authActions.resetPassword.request());
  try {
    yield call(http.post, "/api/userverification/resetPassword", { email }, true);
    yield put(authActions.resetPassword.success());
    yield put(appActions.showMessage("An e-mail with a link to reset your password has been sent."));
  } catch (error) {
    yield put(authActions.resetPassword.failure(error.message));
  }
};

function* resetPasswordCallback({ password, email, token }) {
  yield put(authActions.resetPasswordCallback.request());
  try {
    yield call(http.post, "/api/userverification/resetPasswordCallback", { password, email, token }, true);
    yield put(authActions.resetPasswordCallback.success());
    yield put(authActions.login.invoke(email, password));
  } catch (error) {
    yield put(authActions.resetPasswordCallback.failure(error.message));
  }
};
