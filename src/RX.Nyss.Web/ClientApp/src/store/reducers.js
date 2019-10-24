import { appReducer } from "../components/app/logic/appReducer";
import { authReducer } from "../authentication/authReducer";
import { connectRouter } from 'connected-react-router'
import { combineReducers } from "redux";

export const createRootReducer = (history) => combineReducers({
  router: connectRouter(history),
  appData: appReducer,
  auth: authReducer
});
