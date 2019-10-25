import { appReducer } from "../components/app/logic/appReducer";
import { authReducer } from "../authentication/authReducer";
import { connectRouter } from 'connected-react-router'
import { combineReducers } from "redux";
import { nationalSocietiesReducer } from "../components/nationalSocieties/logic/nationalSocietiesReducer";
import { requestReducer } from "../components/app/logic/requestReducer";

export const createRootReducer = (history) => combineReducers({
  router: connectRouter(history),
  appData: appReducer,
  requests: requestReducer,
  auth: authReducer,
  nationalSocieties: nationalSocietiesReducer
});
