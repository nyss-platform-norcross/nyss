import { nationalSocietiesSiteMap } from "./components/nationalSocieties/nationalSocietiesSiteMap";
import { smsGatewaysSiteMap } from "./components/smsGateways/logic/smsGatewaysSiteMap";
import { globalCoordinatorsSiteMap } from "./components/globalCoordinators/globalCoordinatorsSiteMap";

export const siteMap = [
  ...nationalSocietiesSiteMap,
  ...smsGatewaysSiteMap,
  ...globalCoordinatorsSiteMap
];