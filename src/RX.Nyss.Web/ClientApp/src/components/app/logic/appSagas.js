import { call, put, takeEvery, takeLatest, select, delay } from "redux-saga/effects";
import * as consts from "./appConstans";
import * as authConsts from "../../../authentication/authConstants";
import * as actions from "./appActions";
import { updateStrings, toggleStringsMode } from "../../../strings";
import * as http from "../../../utils/http";
import { placeholders } from "../../../siteMapPlaceholders";
import { getBreadcrumb, getMenu } from "../../../utils/siteMapService";
import * as cache from "../../../utils/cache";
import { reloadPage } from "../../../utils/page";
import * as localStorage from "../../../utils/localStorage";

export const appSagas = () => [
  takeEvery(consts.INIT_APPLICATION.INVOKE, initApplication),
  takeEvery(consts.OPEN_MODULE.INVOKE, openModule),
  takeEvery(consts.ENTITY_UPDATED, entityUpdated),
  takeEvery(consts.SWITCH_STRINGS, switchStrings),
  takeLatest("*", checkLogin)
];

function* initApplication() {
  yield put(actions.initApplication.request());
  try {
    const user = yield select(state => state.appData.user);
    yield call(getAppData);
    yield call(getStrings, user ? user.languageCode : "en");
    yield put(actions.initApplication.success());

    if (user && user.hasPendingNationalSocietyConsents){
      yield put(actions.goToNationalSocietyConsents())
    }

    storeUser(user);
  } catch (error) {
    yield put(actions.initApplication.failure(error.message));
  }
};

function storeUser(user) {
  if (user) {
    localStorage.set(authConsts.localStorageUserIdKey, user.id);
  } else {
    localStorage.remove(authConsts.localStorageUserIdKey);
  }
}

function* checkLogin() {
  const authCookieExpiration = yield select(state => state.appData.authCookieExpiration);

  if (!authCookieExpiration)
  {
    return;
  }

  yield delay((authCookieExpiration + 1) * 60 * 1000);
  reloadPage();
}

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
  const route = yield select(state => state.appData.route);
  path = path || (yield select(state => route && route.path));

  const user = yield select(state => state.appData.user);

  const routeParams = (route && route.params) || {};
  const menuParams = { ...routeParams, ...params };

  const breadcrumb = getBreadcrumb(path, menuParams, user);
  const topMenu = getMenu("/", menuParams, placeholders.topMenu, path, user);
  const sideMenu = getMenu(path, menuParams, placeholders.leftMenu, path, user);
  const tabMenu = getMenu(path, menuParams, placeholders.tabMenu, path, user);

  yield put(actions.openModule.success(path, menuParams, breadcrumb, topMenu, sideMenu, tabMenu, params.title))
}

function* getAppData() {
  yield put(actions.getAppData.request());
  try {
    const appData = yield call(http.get, "/api/appData/getAppData", true);
    yield put(actions.getAppData.success(appData.value.contentLanguages, appData.value.countries, appData.value.isDevelopment, appData.value.isDemo, appData.value.authCookieExpiration));
    return appData.value;
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