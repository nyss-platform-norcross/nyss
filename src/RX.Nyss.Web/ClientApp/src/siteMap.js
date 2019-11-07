import { nationalSocietiesSiteMap } from "./components/nationalSocieties/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";
import { nationalSocietyUsersSiteMap } from "./components/nationalSocietyUsers/logic/nationalSocietyUsersSiteMap";
import { dataCollectorsSiteMap } from "./components/dataCollectors/logic/dataCollectorsSiteMap";
import { projectsSiteMap } from "./components/projects/projectsSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...nationalSocietyUsersSiteMap,
  ...smsGatewaysSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap,
  ...projectsSiteMap,
  ...dataCollectorsSiteMap
];