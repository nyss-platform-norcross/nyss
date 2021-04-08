import { all } from "redux-saga/effects";
import { autoRestart } from "../utils/sagaEffects";
import { appSagas } from "../components/app/logic/appSagas";
import { authSagas } from "../authentication/authSagas";
import { nationalSocietiesSagas } from "../components/nationalSocieties/logic/nationalSocietiesSagas";
import { smsGatewaysSagas } from "../components/smsGateways/logic/smsGatewaysSagas";
import { projectsSagas } from "../components/projects/logic/projectsSagas";
import { globalCoordinatorsSagas } from "../components/globalCoordinators/logic/globalCoordinatorsSagas";
import { healthRisksSagas } from "../components/healthRisks/logic/healthRisksSagas";
import { nationalSocietyUsersSagas } from "../components/nationalSocietyUsers/logic/nationalSocietyUsersSagas";
import { dataCollectorsSagas } from "../components/dataCollectors/logic/dataCollectorsSagas";
import { agreementsSagas } from "../components/agreements/logic/agreementsSagas";
import { reportsSagas } from "../components/reports/logic/reportsSagas";
import { nationalSocietyReportsSagas } from "../components/nationalSocietyReports/logic/nationalSocietyReportsSagas";
import { nationalSocietyStructureSagas } from "../components/nationalSocietyStructure/logic/nationalSocietyStructureSagas";
import { projectDashboardSagas } from "../components/projectDashboard/logic/projectDashboardSagas";
import { alertsSagas } from "../components/alerts/logic/alertsSagas";
import { nationalSocietyDashboardSagas } from "../components/nationalSocietyDashboard/logic/nationalSocietyDashboardSagas";
import { translationsSagas } from "../components/translations/logic/translationsSagas";
import { organizationsSagas } from "../components/organizations/logic/organizationsSagas";
import { projectOrganizationsSagas } from "../components/projectOrganizations/logic/projectOrganizationsSagas";
import { projectAlertRecipientsSagas } from "../components/projectAlertRecipients/logic/projectAlertRecipientsSagas";
import { projectAlertNotHandledRecipientsSagas } from "../components/projectAlertNotHandledRecipient/logic/projectAlertNotHandledRecipientsSagas";

function* rootSaga() {
  yield all([
    ...appSagas(),
    ...authSagas(),
    ...nationalSocietiesSagas(),
    ...nationalSocietyStructureSagas(),
    ...smsGatewaysSagas(),
    ...organizationsSagas(),
    ...projectsSagas(),
    ...projectDashboardSagas(),
    ...projectOrganizationsSagas(),
    ...projectAlertRecipientsSagas(),
    ...globalCoordinatorsSagas(),
    ...healthRisksSagas(),
    ...nationalSocietyUsersSagas(),
    ...dataCollectorsSagas(),
    ...agreementsSagas(),
    ...reportsSagas(),
    ...nationalSocietyReportsSagas(),
    ...nationalSocietyDashboardSagas(),
    ...alertsSagas(),
    ...translationsSagas(),
    ...projectAlertNotHandledRecipientsSagas()
  ]);
}

export const getRootSaga = () =>
  autoRestart(rootSaga);
