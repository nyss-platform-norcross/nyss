import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "../authentication/authConstants";
import * as actions from "../authentication/authActions";
import * as http from "../utils/http";
import * as auth from "./auth";
import { strings } from "../strings";

export const authSagas = () => [
  takeEvery(consts.LOGIN.INVOKE, login),
  takeEvery(consts.LOGOUT.INVOKE, logout),
  takeEvery(consts.VERIFY_EMAIL.INVOKE, verifyEmail)
];

function* login({ userName, password, redirectUrl }) {
  yield put(actions.login.request());
  try {
    const data = yield call(http.post, "/api/authentication/login", { userName, password }, true);

    if (!data.isSuccess) {
      throw new Error(strings(data.message.key));
    }

    auth.setAccessToken(data.value.accessToken)
    yield put(actions.login.success());

    if (redirectUrl) {
      auth.redirectTo(redirectUrl);
    } else {
      auth.redirectToRoot();
    }
  } catch (error) {
    yield put(actions.login.failure(error.message));
  }
};

function* logout() {
  yield put(actions.logout.request());
  try {
    yield call(http.post, "/api/authentication/logout");
    auth.removeAccessToken();
    yield put(actions.logout.success());
    auth.redirectToLogin();
  } catch (error) {
    yield put(actions.logout.failure(error.message));
  }
};

function* verifyEmail({password, email, token}) {
  yield put(actions.verifyEmail.request());
  try {
    yield call(http.post, "/api/userverification/verifyEmailAndAddPassword", {password, email, token}, true);
    yield put(actions.verifyEmail.success());
    auth.redirectToLogin();
  } catch (error) {
    yield put(actions.verifyEmail.failure(error.message));
  }
};
