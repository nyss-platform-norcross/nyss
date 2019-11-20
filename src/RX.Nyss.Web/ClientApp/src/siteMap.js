import { nationalSocietiesSiteMap } from "./components/nationalSocieties/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { projectsSiteMap } from "./components/projects/logic/projectsSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";
import { nationalSocietyUsersSiteMap } from "./components/nationalSocietyUsers/logic/nationalSocietyUsersSiteMap";
import { dataCollectorsSiteMap } from "./components/dataCollectors/logic/dataCollectorsSiteMap";
import { reportsSiteMap } from "./components/reports/logic/reportsSiteMap";
import { nationalSocietyStructureSiteMap } from "./components/nationalSocietyStructure/nationalSocietyStructureSiteMap";
import { projectDashboardSiteMap } from "./components/projectDashboard/logic/projectDashboardSiteMap";
import { reportsSiteMap } from "./components/reports/logic/reportsSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...nationalSocietyStructureSiteMap,
  ...nationalSocietyUsersSiteMap,
  ...smsGatewaysSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap,
  ...projectsSiteMap,
  ...dataCollectorsSiteMap,
  ...projectDashboardSiteMap,
  ...reportsSiteMap
];
