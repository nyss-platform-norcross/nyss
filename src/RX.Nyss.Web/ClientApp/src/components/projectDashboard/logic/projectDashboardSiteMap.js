import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectTabMenuOrder } from "../../projects/logic/projectsSiteMap";

export const projectDashboardSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard",
    title: () => strings(stringKeys.dashboard.title),
    access: accessMap.projects.showDashboard,
    placeholder: placeholders.projectTabMenu,
    placeholderIndex: projectTabMenuOrder.dashboard,
    icon: "Dashboard",
  },
];
