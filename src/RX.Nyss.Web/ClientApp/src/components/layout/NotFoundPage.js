import React, { useState } from "react";
import { useHistory } from "react-router";
import { useDispatch, useSelector } from "react-redux";
import { stringKeys } from "../../strings";
import { BaseLayout } from "./BaseLayout";
import { ROUTE_CHANGED } from "../app/logic/appConstans";
import { trackPageView } from "../../utils/appInsightsHelper";
import { useMount } from "../../utils/lifecycle";

export const NotFoundPage = () => {
  const dispatch = useDispatch();
  const history = useHistory();
  const [error, setError] = useState(stringKeys.error.errorPage.notFound);
  const user = useSelector((state) => state.appData.user);

  useMount(() => {
    // Track page view
    trackPageView("NotFoundPage");
  });

  const returnHome = () => {
    setError(null);
    if(!user){
      history.push("/login");
      dispatch({ type: ROUTE_CHANGED, url: "/login", path: "/login", params: {} });
    } 
    else {
      history.push("/");
      dispatch({ type: ROUTE_CHANGED, url: "/", path: "/", params: {} });
    }
  };

  return <BaseLayout authError={error} returnHome={returnHome} />;
};
