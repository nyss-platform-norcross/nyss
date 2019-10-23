import React from "react";
import * as auth from "../../authentication/auth";
import { Route, Redirect } from "react-router";

export const AuthRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => {
    if (!auth.isAccessTokenSet()) {
      auth.setRedirectUrl(window.location.pathname);
      return <Redirect to={auth.loginUrl} />;
    }

    const redirectUrl = auth.getRedirectUrl();

    if (redirectUrl) {
      auth.removeRedirectUrl();
      return <Redirect to={redirectUrl} />;
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
