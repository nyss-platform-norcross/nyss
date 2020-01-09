import { performanceStatus } from "./dataCollectorsConstants";

export const getBirthDecades = () => {
  const yearMax = Math.floor(new Date().getFullYear() / 10) * 10;
  return Array.from({ length: 10 }, (_, i) => String(yearMax - (10 * i)));
}

export const getIconFromStatus = (status) => {
  switch (status) {
    case performanceStatus.reportingCorrectly: return "check";
    case performanceStatus.reportingWithErrors: return "close";
    case performanceStatus.notReporting: return "access_time";
    default: return "contact_support";
  }
}
