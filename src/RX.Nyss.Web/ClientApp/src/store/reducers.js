import { appReducer } from "../components/app/logic/appReducer";
import { authReducer } from "../authentication/authReducer";
import { connectRouter } from 'connected-react-router'
import { combineReducers } from "redux";
import { nationalSocietiesReducer } from "../components/nationalSocieties/logic/nationalSocietiesReducer";
import { smsGatewaysReducer } from "../components/smsGateways/logic/smsGatewaysReducer";
import { requestReducer } from "../components/app/logic/requestReducer";
import { globalCoordinatorsReducer } from "../components/globalCoordinators/logic/globalCoordinatorsReducer";
import { healthRisksReducer } from "../components/healthRisks/logic/healthRisksReducer";

export const createRootReducer = (history) => combineReducers({
  router: connectRouter(history),
  appData: appReducer,
  requests: requestReducer,
  auth: authReducer,
  nationalSocieties: nationalSocietiesReducer,
  smsGateways: smsGatewaysReducer,
  globalCoordinators: globalCoordinatorsReducer,
  healthRisks: healthRisksReducer
});
