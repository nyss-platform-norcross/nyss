import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const eidsrIntegrationSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/settings",
    path: "/nationalsocieties/:nationalSocietyId/eidsrintegration",
    title: () => strings(stringKeys.eidsrIntegration.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.eidsrIntegration.get,
    placeholderIndex: 4,
    middleStepOnly: true,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/eidsrintegration",
    path: "/nationalsocieties/:nationalSocietyId/eidsrintegration/edit",
    title: () => strings(stringKeys.eidsrIntegration.form.editionTitle),
    access: accessMap.eidsrIntegration.edit,
  },
];
