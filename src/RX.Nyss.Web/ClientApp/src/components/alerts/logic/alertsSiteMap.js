import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectLeftMenuOrder } from "../../projects/logic/projectsSiteMap";

export const alertsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/alerts",
    title: () => strings(stringKeys.alerts.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.alerts.list,
    placeholderIndex: projectLeftMenuOrder.alerts
  }
];
