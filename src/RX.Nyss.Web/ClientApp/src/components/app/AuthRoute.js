import { useEffect, useState } from "react";
import * as auth from "../../authentication/auth";
import { Route, useHistory } from "react-router";
import { BaseLayout } from "../layout/BaseLayout";
import { useDispatch, useSelector } from "react-redux";
import { stringKeys } from "../../strings";
import { ROUTE_CHANGED } from "./logic/appConstans";

export const AuthRoute = ({
  component: Component,
  roles,
  computedMatch,
  ignoreRedirection,
  ...rest
}) => {
  const user = useSelector((state) => state.appData.user);
  const [authError, setAuthError] = useState(null);
  const dispatch = useDispatch();
  const history = useHistory();

  useEffect(() => {
    if (!user) {
      auth.setRedirectUrl(window.location.pathname);
      history.push(auth.loginUrl);
      return;
    }

    const redirectUrl = auth.getRedirectUrl();

    if (redirectUrl && !ignoreRedirection) {
      auth.removeRedirectUrl();
      history.push(redirectUrl);
      return;
    }

    if (
      roles &&
      roles.length &&
      !roles.some((neededRole) =>
        user.roles.some((userRole) => userRole === neededRole),
      )
    ) {
      setAuthError(stringKeys.error.unauthorized);
      return;
    }

    dispatch({
      type: ROUTE_CHANGED,
      url: computedMatch.url,
      path: computedMatch.path,
      params: computedMatch.params,
    });
  }, [user, computedMatch, ignoreRedirection, roles, dispatch, history]);

  const returnHome = () => {
    setAuthError(null);
    history.push("/");
    dispatch({ type: ROUTE_CHANGED, url: "/", path: "/", params: {} });
  };

  if (!!authError) {
    return (
      <BaseLayout authError={authError} returnHome={returnHome}></BaseLayout>
    );
  }

  return <Route exact {...rest} render={(props) => <Component {...props} />} />;
};
