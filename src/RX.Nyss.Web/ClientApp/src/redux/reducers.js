import { appReducer } from "../components/app/logic/appReducer";
import { authReducer } from "../authentication/authReducer";
import { connectRouter } from "connected-react-router";
import { combineReducers } from "redux";
import { nationalSocietiesReducer } from "../components/nationalSocieties/logic/nationalSocietiesReducer";
import { smsGatewaysReducer } from "../components/smsGateways/logic/smsGatewaysReducer";
import { projectsReducer } from "../components/projects/logic/projectsReducer";
import { projectSetupReducer } from "../components/projectSetup/logic/projectSetupReducer";
import { requestReducer } from "../components/app/logic/requestReducer";
import { globalCoordinatorsReducer } from "../components/globalCoordinators/logic/globalCoordinatorsReducer";
import { healthRisksReducer } from "../components/healthRisks/logic/healthRisksReducer";
import { suspectedDiseaseReducer } from "../components/suspectedDiseases/logic/suspectedDiseaseReducer";
import { nationalSocietyUsersReducer } from "../components/nationalSocietyUsers/logic/nationalSocietyUsersReducer";
import { dataCollectorsReducer } from "../components/dataCollectors/logic/dataCollectorsReducer";
import { agreementsReducer } from "../components/agreements/logic/agreementsReducer";
import { reportsReducer } from "../components/reports/logic/reportsReducer";
import { nationalSocietyReportsReducer } from "../components/nationalSocietyReports/logic/nationalSocietyReportsReducer";
import { nationalSocietyStructureReducer } from "../components/nationalSocietyStructure/logic/nationalSocietyStructureReducer";
import { projectDashboardReducer } from "../components/projectDashboard/logic/projectDashboardReducer";
import { alertsReducer } from "../components/alerts/logic/alertsReducer";
import { nationalSocietyDashboardReducer } from "../components/nationalSocietyDashboard/logic/nationalSocietyDashboardReducer";
import { translationsReducer } from "../components/translations/logic/translationsReducer";
import { organizationsReducer } from "../components/organizations/logic/organizationsReducer";
import { projectOrganizationsReducer } from "../components/projectOrganizations/logic/projectOrganizationsReducer";
import { projectAlertRecipientsReducer } from "../components/projectAlertRecipients/logic/projectAlertRecipientsReducer";
import { projectAlertNotHandledRecipientsReducer } from "../components/projectAlertNotHandledRecipient/logic/projectAlertNotHandledRecipientsReducer";
import { alertEventsReducer } from "../components/alertEvents/logic/alertEventsReducer";
import { trackingReducer } from "../utils/tracking";
import { eidsrIntegrationReducer } from "../components/eidsrIntegration/logic/eidsrIntegrationReducer";

export const createRootReducer = (history) =>
  combineReducers({
    router: connectRouter(history),
    appData: appReducer,
    requests: requestReducer,
    auth: authReducer,
    nationalSocieties: nationalSocietiesReducer,
    nationalSocietyStructure: nationalSocietyStructureReducer,
    smsGateways: smsGatewaysReducer,
    eidsrIntegration: eidsrIntegrationReducer,
    organizations: organizationsReducer,
    projects: projectsReducer,
    projectSetup: projectSetupReducer,
    projectDashboard: projectDashboardReducer,
    projectOrganizations: projectOrganizationsReducer,
    projectAlertRecipients: projectAlertRecipientsReducer,
    globalCoordinators: globalCoordinatorsReducer,
    healthRisks: healthRisksReducer,
    suspectedDiseases: suspectedDiseaseReducer,
    nationalSocietyUsers: nationalSocietyUsersReducer,
    dataCollectors: dataCollectorsReducer,
    agreements: agreementsReducer,
    reports: reportsReducer,
    nationalSocietyReports: nationalSocietyReportsReducer,
    nationalSocietyDashboard: nationalSocietyDashboardReducer,
    alerts: alertsReducer,
    alertEvents: alertEventsReducer,
    translations: translationsReducer,
    projectAlertNotHandledRecipients: projectAlertNotHandledRecipientsReducer,
    tracking: trackingReducer,
  });
