import { nationalSocietiesSiteMap } from "./components/nationalSocieties/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/globalCoordinatorsSiteMap";
import { healthRisksSiteMap } from "./components/healthRisks/logic/healthRisksSiteMap";
import { nationalSocietyUsersSiteMap } from "./components/nationalSocietyUsers/logic/nationalSocietyUsersSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...nationalSocietyUsersSiteMap,
  ...smsGatewaysSiteMap,
  ...healthRisksSiteMap,
  ...globalCoordinatorsSiteMap
];