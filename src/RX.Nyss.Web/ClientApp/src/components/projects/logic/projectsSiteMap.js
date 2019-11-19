import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const projectsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: () => strings(stringKeys.project.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.projects.list,
    placeholderIndex: 1
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/add",
    title: () => strings(stringKeys.project.form.creationTitle),
    access: accessMap.projects.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/dashboard",
    title: () => strings(stringKeys.project.dashboard.title),
    access: accessMap.projects.get,
    placeholder: placeholders.leftMenu,
    placeholderIndex: 1
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    title: () => "{projectName}",
    access: accessMap.projects.get
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/edit",
    title: () => strings(stringKeys.project.settings),
    access: accessMap.projects.edit,
    placeholder: placeholders.leftMenu,
    placeholderIndex: 4
  },
];
