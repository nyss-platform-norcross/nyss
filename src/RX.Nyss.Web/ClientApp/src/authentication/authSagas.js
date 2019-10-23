import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "../authentication/authConstants";
import * as actions from "../authentication/authActions";
import * as http from "../utils/http";
import * as auth from "./auth";
import { strings, stringKeys } from "../strings";

export const authSagas = () => [
  takeEvery(consts.LOGIN.INVOKE, login),
  takeEvery(consts.LOGOUT.INVOKE, logout)
];

function* login({ userName, password, redirectUrl }) {
  yield put(actions.login.request());
  try {
    const data = yield call(http.post, "/api/authentication/login", { userName, password }, true);

    if (!data.isSuccess) {
      throw new Error(strings(stringKeys.login.notSucceeded));
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
