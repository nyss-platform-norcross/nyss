import React from "react";
import * as auth from "../../authentication/auth";
import { Route, Redirect } from "react-router";

export const AuthRoute = ({ component: Component, roles, ...rest }) => (
  <Route {...rest} render={props => {
    var tokenData = auth.getAccessTokenData();

    if (!tokenData) {
      auth.setRedirectUrl(window.location.pathname);
      return <Redirect to={auth.loginUrl} />;
    }

    const redirectUrl = auth.getRedirectUrl();

    if (redirectUrl) {
      auth.removeRedirectUrl();
      return <Redirect to={redirectUrl} />;
    }

    if (roles && roles.length && !roles.some(r => r === tokenData.role)) {
      return <div>Not authorized</div>;
    }

    return <Component {...props} />;
  }
  } />
);

export const UnauthorizedRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => auth.isAccessTokenSet()
    ? <Redirect to={auth.rootUrl} />
    : <Component {...props} />
  } />
);
