import { call, put, takeEvery, select } from "redux-saga/effects";
import * as consts from "./appConstans";
import * as actions from "./appActions";
import { updateStrings } from "../../../strings";
import * as http from "../../../utils/http";
import { setAuthorizedFlag, removeAuthorizedFlag, isAuthorized } from "../../../authentication/auth";
import * as cookies from "../../../utils/cookies";
import { push } from "connected-react-router";

export const appSagas = () => [
  takeEvery(consts.INIT_APPLICATION.INVOKE, initApplication)
];

function* initApplication() {
  yield put(actions.initApplication.request());
  try {
    const user = yield call(getUser);

    if (user) {
      yield call(getStrings);
      yield call(getAppData);
      setAuthorizedFlag(user.name);
    }
    else {
      if (isAuthorized()) {
        removeAuthorizedFlag();
        const pathname = yield select(state => state.router.location.pathname);
        yield put(push(pathname));
      }
    }

    yield put(actions.initApplication.success());
  } catch (error) {
    yield put(actions.initApplication.failure(error.message));
  }
};

function* getUser() {
  yield put(actions.getUser.request());
  try {
    const status = yield call(http.post, "/api/authentication/status");

    const user = status.isAuthenticated
      ? { name: status.data.name, roles: status.data.roles }
      : null;

    yield put(actions.getUser.success(status.isAuthenticated, user, cookies.get("idsrv.session")));
    return user;
  } catch (error) {
    yield put(actions.getUser.failure(error.message));
  }
};

function* getAppData() {
  yield put(actions.getAppData.invoke());
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