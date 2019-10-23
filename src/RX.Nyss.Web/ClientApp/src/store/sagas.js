import { all } from "redux-saga/effects";
import { autoRestart } from "../utils/sagaEffects";
import { appSagas } from "../components/app/logic/appSagas";
import { authSagas } from "../authentication/authSagas";

function* rootSaga() {
    yield all([
        ...appSagas(),
        ...authSagas(),
    ]);
}

export const getRootSaga = () =>
    autoRestart(rootSaga);
