import React from "react";
import * as auth from "../../authentication/auth";
import { Route, Redirect } from "react-router";

export const AuthRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => {
    if (!auth.isAuthorized()) {
      auth.setRedirectUrl(window.location.pathname);
      return <Redirect to="/login" />;
    }

    const redirectUri = auth.getRedirectUrl();

    if (redirectUri) {
      auth.removeRedirectUrl();
      if (redirectUri.replace(/\/$/, "") !== window.location.origin) {
        const returnUrl = redirectUri.replace(window.location.origin, "");
        return <Redirect to={returnUrl} />;
      }
    }

    return <Component {...props} />;
  }
  } />
);

export const UnauthorizedRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => auth.isAuthorized()
    ? <Redirect to="/" />
    : <Component {...props} />
  } />
);
