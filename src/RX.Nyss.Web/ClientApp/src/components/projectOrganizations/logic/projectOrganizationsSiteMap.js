import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const projectOrganizationsSiteMap = [
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/organizations",
    title: () => strings(stringKeys.projectOrganization.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.projectOrganizations.list,
    placeholderIndex: 2,
    middleStepOnly: true,
    hideWhen: (parameters) => !parameters.allowMultipleOrganizations,
  },
  {
    parentPath: "/projects/:projectId/organizations",
    path: "/projects/:projectId/organizations/add",
    title: () => strings(stringKeys.projectOrganization.form.creationTitle),
    access: accessMap.projectOrganizations.add,
  },
];
