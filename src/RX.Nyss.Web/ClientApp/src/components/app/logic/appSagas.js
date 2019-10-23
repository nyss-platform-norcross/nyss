import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./appConstans";
import * as actions from "./appActions";
import { updateStrings } from "../../../strings";
import * as http from "../../../utils/http";
import {  removeAccessToken, isAccessTokenSet } from "../../../authentication/auth";
import { push } from "connected-react-router";

export const appSagas = () => [
  takeEvery(consts.INIT_APPLICATION.INVOKE, initApplication)
];

function* initApplication() {
  yield put(actions.initApplication.request());
  try {
    const user = yield call(getAndVerifyUser);

    if (user) {
      yield call(getStrings);
      yield call(getAppData);
    }

    yield put(actions.initApplication.success());
  } catch (error) {
    yield put(actions.initApplication.failure(error.message));
  }
};

function* reloadPage() {
  const pathname = yield select(state => state.router.location.pathname);
  yield put(push(pathname));
}

function* getAndVerifyUser() {
  if (!isAccessTokenSet()) {
    return null;
  }

  const user = yield call(getUserStatus);

  if (!user) {
    removeAccessToken();
    yield reloadPage();
    return null;
  }

  return user;
};

function* getUserStatus() {
  yield put(actions.getUser.request());
  try {
    const status = yield call(http.post, "/api/authentication/status");

    const user = status.isAuthenticated
      ? { name: status.data.name, roles: status.data.roles }
      : null;

    yield put(actions.getUser.success(status.isAuthenticated, user));
    return user;
  } catch (error) {
    yield put(actions.getUser.failure(error.message));
  }
};

function* getAppData() {
  yield put(actions.getAppData.request());
  try {
    yield put(actions.getAppData.success());
  } catch (error) {
    yield put(actions.getAppData.failure(error.message));
  }
};

function* getStrings() {
  yield put(actions.getStrings.invoke());
  try {
    // api call
    updateStrings({});
    yield put(actions.getStrings.success());
  } catch (error) {
    yield put(actions.getStrings.failure(error.message));
  }
};