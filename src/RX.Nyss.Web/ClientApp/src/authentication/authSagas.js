import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "../authentication/authConstants";
import * as actions from "../authentication/authActions";
import * as http from "../utils/http";
import * as auth from "./auth";
import { push } from "connected-react-router";

export const authSagas = () => [
  takeEvery(consts.LOGIN.INVOKE, login),
  takeEvery(consts.LOGOUT.INVOKE, logout)
];

function* login({ userName, password, returnUrl }) {
  yield put(actions.login.request());
  try {
    yield call(http.post, "/api/authentication/login", { userName, password });
    yield put(actions.login.success());

    if (returnUrl) {
      push(returnUrl);
    }
  } catch (error) {
    yield put(actions.login.failure(error.message));
  }
};

function* logout() {
  yield put(actions.logout.invoke());
  try {
    yield call(http.post, "/api/authentication/logout");
    auth.removeAuthorizedFlag();
    yield put(actions.logout.success());
  } catch (error) {
    yield put(actions.logout.failure(error.message));
  }
};
