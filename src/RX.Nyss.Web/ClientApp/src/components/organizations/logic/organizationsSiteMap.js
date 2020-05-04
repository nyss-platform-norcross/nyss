import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const organizationsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/settings",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways",
    title: () => strings(stringKeys.organization.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.organizations.list,
    placeholderIndex: 2,
    middleStepOnly: true
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/add",
    title: () => strings(stringKeys.organization.form.creationTitle),
    access: accessMap.organizations.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/:organizationId/edit",
    title: () => strings(stringKeys.organization.form.editionTitle),
    access: accessMap.organizations.edit
  }
];
