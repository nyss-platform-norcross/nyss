import * as http from '../../../utils/http';
import * as appActions from './appActions';
import { actions } from "../../../utils/tracking";

export const runApplication = (dispatch) =>
  http.get("/api/authentication/status")
    .then(status => {
      const user = status.value.isAuthenticated
        ? status.value.userData
        : null;

      dispatch(appActions.getUser.success(status.value.isAuthenticated, user));
      dispatch(appActions.initApplication.invoke());
      dispatch(actions.initTracking());
    })
    .catch(() => {
      dispatch(appActions.initApplication.invoke());
    });
