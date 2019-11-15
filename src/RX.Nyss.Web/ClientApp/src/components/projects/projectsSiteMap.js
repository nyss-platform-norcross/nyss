import { accessMap } from "../../authentication/accessMap";
import { placeholders } from "../../siteMapPlaceholders";

export const projectsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: () => "Projects",
    placeholder: placeholders.leftMenu,
    placeholderIndex: 5,
    access: accessMap.projects.list
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    title: () => "{projectName}",
    access: accessMap.projects.edit
  }
];
