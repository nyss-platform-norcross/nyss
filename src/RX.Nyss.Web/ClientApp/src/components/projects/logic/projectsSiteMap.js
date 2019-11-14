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
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/edit",
    title: () => strings(stringKeys.project.form.editionTitle),
    access: accessMap.projects.edit
  }
];
