import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectLeftMenuOrder } from "../../projects/logic/projectsSiteMap";

export const reportsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/reports",
    title: () => strings(stringKeys.reports.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.reports.list,
    placeholderIndex: projectLeftMenuOrder.reports
  }
];
