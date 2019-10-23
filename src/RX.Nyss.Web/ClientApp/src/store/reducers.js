import { appReducer } from "../components/app/logic/appReducer";
import { authReducer } from "../authentication/authReducer";

export const rootReducer = ({
  appData: appReducer,
  auth: authReducer
});
