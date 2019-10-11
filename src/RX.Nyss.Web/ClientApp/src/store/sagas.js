import { all } from "redux-saga/effects";
import { autoRestart } from "../utils/sagaEffects";
import { appSagas } from "../components/app/logic/appSagas";

function* rootSaga() {
    yield all([
        ...appSagas(),
    ]);
}

export const getRootSaga = () =>
    autoRestart(rootSaga);
