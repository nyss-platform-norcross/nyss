import { all } from "redux-saga/effects";
import { autoRestart } from "../utils/sagaEffects";
import { appSagas } from "../components/app/logic/appSagas";
import { authSagas } from "../authentication/authSagas";
import { nationalSocietiesSagas } from "../components/nationalSocieties/logic/nationalSocietiesSagas";
import { globalCoordinatorsSagas } from "../components/globalCoordinators/logic/globalCoordinatorsSagas";

function* rootSaga() {
  yield all([
    ...appSagas(),
    ...authSagas(),
    ...nationalSocietiesSagas(),
    ...globalCoordinatorsSagas()
  ]);
}

export const getRootSaga = () =>
  autoRestart(rootSaga);
