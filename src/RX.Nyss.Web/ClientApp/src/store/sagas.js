import { all } from "redux-saga/effects";
import { autoRestart } from "../utils/sagaEffects";
import { appSagas } from "../components/app/logic/appSagas";
import { authSagas } from "../authentication/authSagas";
import { nationalSocietiesSagas } from "../components/nationalSocieties/logic/nationalSocietiesSagas";
import { smsGatewaysSagas } from "../components/smsGateways/logic/smsGatewaysSagas";
import { globalCoordinatorsSagas } from "../components/globalCoordinators/logic/globalCoordinatorsSagas";
import { healthRisksSagas } from "../components/healthRisks/logic/healthRisksSagas";
import { nationalSocietyUsersSagas } from "../components/nationalSocietyUsers/logic/nationalSocietyUsersSagas";
import { dataCollectorsSagas } from "../components/dataCollectors/logic/dataCollectorsSagas";

function* rootSaga() {
  yield all([
    ...appSagas(),
    ...authSagas(),
    ...nationalSocietiesSagas(),
    ...smsGatewaysSagas(),
    ...globalCoordinatorsSagas(),
    ...healthRisksSagas(),
    ...nationalSocietyUsersSagas(),
    ...dataCollectorsSagas()
  ]);
}

export const getRootSaga = () =>
  autoRestart(rootSaga);
