import React, { useEffect, useState } from "react";
import { ApplicationInsights } from "@microsoft/applicationinsights-web";

let ai;

export const ApplicationInsightsProvider = ({ connectionString, children }) => {
  const [activated, setActivated] = useState(false);

  useEffect(() => {
    if (!activated && connectionString) {
      ai = new ApplicationInsights({
        config: {
          connectionString: connectionString,
          autoTrackPageVisitTime: true,
        },
      });
      ai.loadAppInsights();

      setActivated(true);
    }
  }, [activated, connectionString]);

  return <>{children}</>;
};

export const getAppInsights = () => ai;
