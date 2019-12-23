import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const nationalSocietyLeftMenuOrder = {
  dashboard: 0,
  projects: 10,
  userlist: 20,
  nationalSocietyReports: 30,
  settings: 40
};

export const nationalSocietiesSiteMap = [
  {
    path: "/nationalsocieties",
    title: () => strings(stringKeys.nationalSociety.title),
    placeholder: placeholders.topMenu,
    access: accessMap.nationalSocieties.list
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/add",
    title: () => strings(stringKeys.nationalSociety.form.creationTitle),
    access: accessMap.nationalSocieties.add
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/:nationalSocietyId",
    title: () => "{nationalSocietyName}",
    access: accessMap.nationalSocieties.get
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/dashboard",
    title: () => strings(stringKeys.nationalSociety.dashboard.title),
    access: accessMap.nationalSocieties.get,
    placeholder: placeholders.leftMenu,
    placeholderIndex: nationalSocietyLeftMenuOrder.dashboard,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/overview",
    path: "/nationalsocieties/:nationalSocietyId/edit",
    title: () => strings(stringKeys.nationalSociety.form.editionTitle),
    access: accessMap.nationalSocieties.edit
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/settings",
    title: () => strings(stringKeys.nationalSociety.settings.title),
    placeholder: placeholders.leftMenu,
    placeholderIndex: nationalSocietyLeftMenuOrder.settings,
    access: accessMap.nationalSocieties.edit
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/settings",
    path: "/nationalsocieties/:nationalSocietyId/overview",
    title: () => strings(stringKeys.nationalSociety.overview.title),
    placeholder: placeholders.tabMenu,
    placeholderIndex: 1,
    access: accessMap.nationalSocieties.edit,
    middleStepOnly: true
  }
];
