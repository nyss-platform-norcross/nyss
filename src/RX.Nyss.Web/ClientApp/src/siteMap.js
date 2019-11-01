import { nationalSocietiesSiteMap } from "./components/nationalSocieties/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...smsGatewaysSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap
];