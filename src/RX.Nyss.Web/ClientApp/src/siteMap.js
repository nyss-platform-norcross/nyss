import { nationalSocietiesSiteMap } from "./components/nationalSocieties/logic/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { projectsSiteMap } from "./components/projects/logic/projectsSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/logic/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";
import { nationalSocietyUsersSiteMap } from "./components/nationalSocietyUsers/logic/nationalSocietyUsersSiteMap";
import { dataCollectorsSiteMap } from "./components/dataCollectors/logic/dataCollectorsSiteMap";
import { reportsSiteMap } from "./components/reports/logic/reportsSiteMap";
import { nationalSocietyReportsSiteMap } from "./components/nationalSocietyReports/logic/nationalSocietyReportsSiteMap";
import { nationalSocietyStructureSiteMap } from "./components/nationalSocietyStructure/logic/nationalSocietyStructureSiteMap";
import { projectDashboardSiteMap } from "./components/projectDashboard/logic/projectDashboardSiteMap";
import { headManagerConsentsSiteMap } from "./components/headManagerConsents/logic/headManagerConsentsSiteMap";
import { alertsSiteMap } from "./components/alerts/logic/alertsSiteMap";
import { nationalSocietyDashboardSiteMap } from "./components/nationalSocietyDashboard/logic/nationalSocietyDashboardSiteMap";
import { translationsSiteMap } from "./components/translations/logic/translationsSiteMap";
import { organizationsSiteMap } from "./components/organizations/logic/organizationsSiteMap";
import { projectOrganizationsSiteMap } from "./components/projectOrganizations/logic/projectOrganizationsSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...nationalSocietyDashboardSiteMap,
  ...nationalSocietyStructureSiteMap,
  ...nationalSocietyUsersSiteMap,
  ...smsGatewaysSiteMap,
  ...organizationsSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap,
  ...projectsSiteMap,
  ...projectDashboardSiteMap,
  ...projectOrganizationsSiteMap,
  ...dataCollectorsSiteMap,
  ...reportsSiteMap,
  ...nationalSocietyReportsSiteMap,
  ...headManagerConsentsSiteMap,
  ...alertsSiteMap,
  ...translationsSiteMap
];
