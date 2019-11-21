import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const projectDashboardSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard",
    title: () => strings(stringKeys.project.dashboard.title),
    access: accessMap.projects.get,
    placeholder: placeholders.leftMenu,
    placeholderIndex: 1
  }
];
