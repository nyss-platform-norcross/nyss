import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "./translationsConstants";
import * as actions from "./translationsActions";
import * as http from "../../../utils/http";
import * as appActions from "../../app/logic/appActions";
import { strings, stringKeys } from "../../../strings";

export const translationsSagas = () => [
  takeEvery(consts.OPEN_TRANSLATIONS_LIST.INVOKE, openTranslationsList)
];

function* openTranslationsList({ path, params }) {
  yield put(actions.openList.request());
  try {
    yield call(getTranslations);
    yield put(appActions.openModule.invoke(path, {title: strings(stringKeys.translations.title)}));
    yield put(actions.openList.success());
  } catch (error) {
    yield put(actions.openList.failure(error.message));
  }
};

function* getTranslations() {
  yield put(actions.getList.request());
  try {
    const response = yield call(http.get, `/api/resources/listTranslations`);
    yield put(actions.getList.success(response.value));
  } catch (error) {
    yield put(actions.getList.failure(error.message));
  }
};
