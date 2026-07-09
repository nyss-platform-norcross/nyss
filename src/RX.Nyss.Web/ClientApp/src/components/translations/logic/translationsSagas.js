import { call, put, takeEvery } from "redux-saga/effects";
import * as consts from "./translationsConstants";
import * as actions from "./translationsActions";
import * as http from "../../../utils/http";
import * as appActions from "../../app/logic/appActions";
import { strings, stringKeys } from "../../../strings";

export const translationsSagas = () => [
  takeEvery(consts.OPEN_TRANSLATIONS_LIST.INVOKE, openTranslationsList),
  takeEvery(
    consts.OPEN_EMAIL_TRANSLATIONS_LIST.INVOKE,
    openEmailTranslationsList,
  ),
  takeEvery(consts.OPEN_SMS_TRANSLATIONS_LIST.INVOKE, openSmsTranslationsList),
  takeEvery(consts.GET_TRANSLATIONS.INVOKE, getTranslations),
  takeEvery(consts.GET_EMAIL_TRANSLATIONS.INVOKE, getEmailTranslations),
  takeEvery(consts.GET_SMS_TRANSLATIONS.INVOKE, getSmsTranslations),
];

function* openTranslationsList({ path }) {
  yield put(actions.openTranslationsList.request());
  try {
    yield call(getTranslations, { needsImprovementOnly: false });
    yield put(
      appActions.openModule.invoke(path, {
        title: strings(stringKeys.translations.title),
      }),
    );
    yield put(actions.openTranslationsList.success());
  } catch (error) {
    yield put(actions.openTranslationsList.failure(error.message));
  }
}

function* getTranslations({ needsImprovementOnly }) {
  yield put(actions.getTranslationsList.request());
  try {
    const response = yield call(
      http.get,
      `/api/resources/listStringsTranslations?needsImprovementOnly=${needsImprovementOnly}`,
    );
    yield put(actions.getTranslationsList.success(response.value));
  } catch (error) {
    yield put(actions.getTranslationsList.failure(error.message));
  }
}

function* openEmailTranslationsList({ path }) {
  yield put(actions.openEmailTranslationsList.request());
  try {
    yield call(getEmailTranslations, { needsImprovementOnly: false });
    yield put(
      appActions.openModule.invoke(path, {
        title: strings(stringKeys.translations.title),
      }),
    );
    yield put(actions.openEmailTranslationsList.success());
  } catch (error) {
    yield put(actions.openEmailTranslationsList.failure(error.message));
  }
}

function* getEmailTranslations({ needsImprovementOnly }) {
  yield put(actions.getEmailTranslationsList.request());
  try {
    const response = yield call(
      http.get,
      `/api/resources/listEmailTranslations?needsImprovementOnly=${needsImprovementOnly}`,
    );
    yield put(actions.getEmailTranslationsList.success(response.value));
  } catch (error) {
    yield put(actions.getEmailTranslationsList.failure(error.message));
  }
}

function* openSmsTranslationsList({ path }) {
  yield put(actions.openSmsTranslationsList.request());
  try {
    yield call(getSmsTranslations, { needsImprovementOnly: false });
    yield put(
      appActions.openModule.invoke(path, {
        title: strings(stringKeys.translations.title),
      }),
    );
    yield put(actions.openSmsTranslationsList.success());
  } catch (error) {
    yield put(actions.openSmsTranslationsList.failure(error.message));
  }
}

function* getSmsTranslations({ needsImprovementOnly }) {
  yield put(actions.getSmsTranslationsList.request());
  try {
    const response = yield call(
      http.get,
      `/api/resources/listSmsTranslations?needsImprovementOnly=${needsImprovementOnly}`,
    );
    yield put(actions.getSmsTranslationsList.success(response.value));
  } catch (error) {
    yield put(actions.getSmsTranslationsList.failure(error.message));
  }
}
