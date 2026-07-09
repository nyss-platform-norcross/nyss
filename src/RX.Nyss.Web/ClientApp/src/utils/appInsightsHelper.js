import { getAppInsights } from "../components/app/ApplicationInsightsProvider";
import store from "../store";
import { deviceType } from "react-device-detect";

export const trackPageView = (name, properties) => {
  const appData = store.getState().appData;
  const userRole = appData?.user?.roles[0];
  const nationalSocietyName = appData?.siteMap?.parameters?.nationalSocietyName;
  const projectName = appData?.siteMap?.parameters?.projectName;

  const appInsights = getAppInsights();
  appInsights?.trackPageView({
    name,
    properties: {
      userRole,
      deviceType,
      nationalSocietyName,
      projectName,
      ...properties,
    },
  });
};

export const trackEvent = (name, properties) => {
  const appData = store.getState().appData;
  const userRole = appData?.user?.roles[0];
  const nationalSocietyName = appData?.siteMap?.parameters?.nationalSocietyName;
  const projectName = appData?.siteMap?.parameters?.projectName;

  const appInsights = getAppInsights();
  appInsights?.trackEvent({
    name,
    properties: {
      userRole,
      deviceType,
      nationalSocietyName,
      projectName,
      ...properties,
    },
  });
};
