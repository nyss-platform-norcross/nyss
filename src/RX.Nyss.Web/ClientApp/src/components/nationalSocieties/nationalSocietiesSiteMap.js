import { placeholders } from "../../siteMapPlaceholders";
import { accessMap } from "../../authentication/accessMap";
import { Administrator, GlobalCoordinator, DataConsumer, Manager } from "../../authentication/roles";
import { strings, stringKeys } from "../../strings";

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
    title: () => "{nationalSocietyName} ({nationalSocietyCountry})",
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/dashboard",
    title: () => strings(stringKeys.nationalSociety.dashboard.title),
    access: [Administrator, GlobalCoordinator, DataConsumer],
    placeholder: placeholders.leftMenu,
    placeholderIndex: 1,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/overview",
    path: "/nationalsocieties/:nationalSocietyId/edit",
    title: () => strings(stringKeys.nationalSociety.form.editionTitle),
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/overview",
    title: () => strings(stringKeys.nationalSociety.overview.title),
    placeholder: placeholders.leftMenu,
    placeholderIndex: 4,
    access: [Administrator, GlobalCoordinator, Manager, DataConsumer]
  }
];
