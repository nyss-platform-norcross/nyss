import { call, put, takeEvery, select, delay } from "redux-saga/effects";
import * as consts from "./appConstans";
import * as actions from "./appActions";
import { updateStrings, toggleStringsMode } from "../../../strings";
import * as http from "../../../utils/http";
import {  removeAccessToken, isAccessTokenSet } from "../../../authentication/auth";
import { push } from "connected-react-router";
import { placeholders } from "../../../siteMapPlaceholders";
import { getBreadcrumb, getMenu } from "../../../utils/siteMapService";
import * as cache from "../../../utils/cache";

export const appSagas = () => [
  takeEvery(consts.INIT_APPLICATION.INVOKE, initApplication),
  takeEvery(consts.OPEN_MODULE.INVOKE, openModule),
  takeEvery(consts.ENTITY_UPDATED, entityUpdated),
  takeEvery(consts.SWITCH_STRINGS, switchStrings),
];

function* initApplication() {
  yield put(actions.initApplication.request());
  try {
    const user = yield call(getAndVerifyUser);
    yield call(getAppData);
    yield call(getStrings, user ? user.languageCode : "en");
    yield put(actions.initApplication.success());
  } catch (error) {
    yield put(actions.initApplication.failure(error.message));
  }
};

function* switchStrings() {
  yield put(actions.setAppReady(false));
  toggleStringsMode();
  yield delay(1);

  const hasBreadcrumb = yield select(state => state.appData.siteMap.breadcrumb.length !== 0);

  if (hasBreadcrumb) {
    const pathAndParams = yield select(state => ({
      path: state.appData.route.path,
      params: state.appData.route.params
    }));

    yield openModule(pathAndParams);
  }

  yield put(actions.setAppReady(true));
}

function* openModule({ path, params }) {
  path = path || (yield select(state => state.appData.route && state.appData.route.path));

  const breadcrumb = getBreadcrumb(path, params);
  const topMenu = getMenu("/", params, placeholders.topMenu, path);
  const sideMenu = getMenu(path, params, placeholders.leftMenu, path);

  yield put(actions.openModule.success(path, params, breadcrumb, topMenu, sideMenu))
}

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
    const status = yield call(http.get, "/api/authentication/status");

    const user = status.value.isAuthenticated
      ? {
        name: status.value.data.name,
        roles: status.value.data.roles,
        languageCode: status.value.data.languageCode
      }
      : null;

    yield put(actions.getUser.success(status.value.isAuthenticated, user));
    return user;
  } catch (error) {
    yield put(actions.getUser.failure(error.message));
  }
};

function* getAppData() {
  yield put(actions.getAppData.request());
  try {
    const appData = yield call(http.get, "/api/appData/getAppData", true);
    yield put(actions.getAppData.success(appData.value.contentLanguages, appData.value.countries, appData.value.isDevelopment));
  } catch (error) {
    yield put(actions.getAppData.failure(error.message));
  }
};

function* getStrings(languageCode) {
  yield put(actions.getStrings.invoke());
  try {
    const response = yield call(http.get, `/api/appData/getStrings/${languageCode}`, true);
    updateStrings(response.value);
    yield put(actions.getStrings.success());
  } catch (error) {
    yield put(actions.getStrings.failure(error.message));
  }
};

function entityUpdated({ entities }) {
  cache.cleanCacheForDependencies(entities);
};