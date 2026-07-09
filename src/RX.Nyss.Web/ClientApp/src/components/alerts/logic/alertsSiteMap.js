import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectTabMenuOrder } from "../../projects/logic/projectsSiteMap";

export const alertsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/alerts",
    title: () => strings(stringKeys.alerts.title),
    placeholder: placeholders.projectTabMenu,
    access: accessMap.alerts.list,
    placeholderIndex: projectTabMenuOrder.alerts,
    icon: "Alerts"
  },
  {
    parentPath: "/projects/:projectId/alerts",
    path: "/projects/:projectId/alerts/:alertId/details",
    title: () => strings(stringKeys.alerts.details.title),
    access: accessMap.alerts.assess,
  },
  {
    parentPath: "/projects/:projectId/alerts/:alertId/details",
    path: "/projects/:projectId/alerts/:alertId/assess",
    title: () => strings(stringKeys.alerts.assess.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.alerts.assess,
    middleStepOnly: true,
  },
];
