import {
  call,
  put,
  takeEvery,
  takeLatest,
  select,
  delay,
} from "redux-saga/effects";
import * as consts from "./appConstans";
import * as authConsts from "../../../authentication/authConstants";
import * as actions from "./appActions";
import { updateStrings, toggleStringsMode } from "../../../strings";
import * as http from "../../../utils/http";
import { placeholders } from "../../../siteMapPlaceholders";
import { getHierarchy, getMenu } from "../../../utils/siteMapService";
import * as cache from "../../../utils/cache";
import { reloadPage } from "../../../utils/page";
import * as localStorage from "../../../utils/localStorage";
import { initTracking } from "../../../utils/tracking";

export const appSagas = () => [
  takeEvery(consts.INIT_APPLICATION.INVOKE, initApplication),
  takeEvery(consts.OPEN_MODULE.INVOKE, openModule),
  takeEvery(consts.ENTITY_UPDATED, entityUpdated),
  takeEvery(consts.SWITCH_STRINGS, switchStrings),
  takeEvery(consts.SEND_FEEDBACK.INVOKE, sendFeedback),
  takeLatest("*", checkLogin),
];

function* initApplication() {
  yield put(actions.initApplication.request());
  try {
    const user = yield select((state) => state.appData.user);
    yield call(getAppData);
    yield call(getStrings, user ? user.languageCode : "en");
    yield call(initTracking);
    yield put(actions.initApplication.success());

    window.userLanguage = user && user.languageCode;

    storeUser(user);
  } catch (error) {
    yield put(actions.initApplication.failure(error.message));
  }
}

function storeUser(user) {
  if (user) {
    localStorage.set(authConsts.localStorageUserIdKey, user.id);
  } else {
    localStorage.remove(authConsts.localStorageUserIdKey);
  }
}

function* checkLogin() {
  const authCookieExpiration = yield select(
    (state) => state.appData.authCookieExpiration,
  );

  if (!authCookieExpiration) {
    return;
  }

  yield delay((authCookieExpiration + 1) * 60 * 1000);
  reloadPage();
}

function* switchStrings() {
  yield put(actions.setAppReady(false));
  toggleStringsMode();
  yield delay(1);

  yield put(actions.setAppReady(true));
}

function* openModule({ path, params }) {
  const route = yield select((state) => state.appData.route);
  path = path || (yield select((state) => route && route.path));

  const user = yield select((state) => state.appData.user);

  const routeParams = (route && route.params) || {};
  const menuParams = { ...routeParams, ...params };

  const generalMenu = getMenu(menuParams, placeholders.generalMenu, path, user);
  const sideMenu = getMenu(menuParams, placeholders.leftMenu, path, user);
  const tabMenu = getMenu(menuParams, placeholders.tabMenu, path, user);
  const projectTabMenu = getMenu(
    menuParams,
    placeholders.projectTabMenu,
    path,
    user,
  ).map((item) => ({
    ...item,
    subMenu: getMenu(
      menuParams,
      placeholders.projectSubMenu,
      path,
      user,
      item.path,
    ),
  }));
  const title =
    params.title || getHierarchy(path, menuParams, user).slice(-1)[0].title;

  yield put(
    actions.openModule.success(
      path,
      menuParams,
      generalMenu,
      sideMenu,
      tabMenu,
      projectTabMenu,
      title,
    ),
  );
}

function* getAppData() {
  yield put(actions.getAppData.request());
  try {
    const appData = yield call(http.get, "/api/appData/getAppData", true);
    yield put(
      actions.getAppData.success(
        appData.value.contentLanguages,
        appData.value.countries,
        appData.value.isDevelopment,
        appData.value.isDemo,
        appData.value.authCookieExpiration,
        appData.value.applicationInsightsConnectionString,
      ),
    );
  } catch (error) {
    yield put(actions.getAppData.failure(error.message));
  }
}

function* getStrings(languageCode) {
  yield put(actions.getStrings.invoke());
  try {
    const response = yield call(
      http.get,
      `/api/appData/getStrings/${languageCode}`,
      true,
    );
    updateStrings(response.value);
    yield put(actions.getStrings.success());
  } catch (error) {
    yield put(actions.getStrings.failure(error.message));
  }
}

function entityUpdated({ entities }) {
  cache.cleanCacheForDependencies(entities);
}

function* sendFeedback({ message }) {
  yield put(actions.sendFeedback.request());
  try {
    yield call(http.post, `/api/feedback`, message);
    yield put(actions.sendFeedback.success());
  } catch (error) {
    yield put(actions.sendFeedback.failure(error.message));
  }
}
