import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const projectSetupSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/setup",
    title: () => strings(stringKeys.project.form.creationTitle),
    access: accessMap.projects.add,
  },
];
