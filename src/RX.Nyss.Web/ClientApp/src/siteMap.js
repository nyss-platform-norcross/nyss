import { nationalSocietiesSiteMap } from "./components/nationalSocieties/logic/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { projectsSiteMap } from "./components/projects/logic/projectsSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/logic/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";
import { nationalSocietyUsersSiteMap } from "./components/nationalSocietyUsers/logic/nationalSocietyUsersSiteMap";
import { dataCollectorsSiteMap } from "./components/dataCollectors/logic/dataCollectorsSiteMap";
import { reportsSiteMap } from "./components/reports/logic/reportsSiteMap";
import { nationalSocietyStructureSiteMap } from "./components/nationalSocietyStructure/logic/nationalSocietyStructureSiteMap";
import { projectDashboardSiteMap } from "./components/projectDashboard/logic/projectDashboardSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...nationalSocietyStructureSiteMap,
  ...nationalSocietyUsersSiteMap,
  ...smsGatewaysSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap,
  ...projectsSiteMap,
  ...projectDashboardSiteMap,
  ...dataCollectorsSiteMap,
  ...reportsSiteMap
];
