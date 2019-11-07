import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const nationalSocietyUsersSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/users",
    title: () => strings(stringKeys.nationalSocietyUser.title),
    placeholder: placeholders.leftMenu,
    placeholderIndex: 2,
    access: accessMap.nationalSocietyUsers.list
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/add",
    title: () => strings(stringKeys.nationalSocietyUser.form.creationTitle),
    access: accessMap.nationalSocietyUsers.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit",
    title: () => strings(stringKeys.nationalSocietyUser.form.editionTitle),
    access: accessMap.nationalSocietyUsers.edit
  }
];
