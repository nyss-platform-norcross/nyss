import React from "react";
import * as auth from "../../authentication/auth";
import { Route, Redirect } from "react-router";
import { BaseLayout } from "../layout/BaseLayout";
import { ReactReduxContext } from 'react-redux'

export const AuthRoute = ({ component: Component, roles, computedMatch, ...rest }) => (
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
      return <BaseLayout authError={"Not authorized"}></BaseLayout>;
    }

    return <ReactReduxContext.Consumer>
      {({ store }) => {
        store.dispatch({ type: "ROUTE_CHANGED", url: computedMatch.url, path: computedMatch.path, params: computedMatch.params })
        return <Component {...props} />;
      }}
    </ReactReduxContext.Consumer>;
  }
  } />
);

export const UnauthorizedRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => auth.isAccessTokenSet()
    ? <Redirect to={auth.rootUrl} />
    : <Component {...props} />
  } />
);
