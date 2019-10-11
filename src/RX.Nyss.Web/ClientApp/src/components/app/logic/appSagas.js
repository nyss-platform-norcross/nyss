import { call, put, select, takeEvery, delay } from "redux-saga/effects";
import * as actions from "./appConstans";
import { updateStrings } from "../../../strings";

export const appSagas = () => [
    takeEvery(actions.INIT_APPLICATION.INVOKE, initApplication)
];

export function* initApplication() {
    yield put({ type: actions.INIT_APPLICATION.REQUEST });
    try {
        yield call(populateStrings);
        yield put({ type: actions.INIT_APPLICATION.SUCCESS });
    } catch (error) {
        yield put({ type: actions.INIT_APPLICATION.FAILURE, message: error.message });
    }
};

function* populateStrings() {
    yield put({ type: actions.GET_APP_DATA.REQUEST });
    try {
        yield delay(1000);
        updateStrings({});

        yield put({ type: actions.GET_APP_DATA.SUCCESS });
    } catch (error) {
        yield put({ type: actions.GET_APP_DATA.FAILURE, message: error.message });
    }
};
