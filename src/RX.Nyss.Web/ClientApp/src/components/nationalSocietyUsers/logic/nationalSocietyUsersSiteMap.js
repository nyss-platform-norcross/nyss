import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";

export const nationalSocietyUsersSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/users",
    title: () => strings(stringKeys.nationalSocietyUser.title),
    placeholder: placeholders.leftMenu,
    placeholderIndex: nationalSocietyLeftMenuOrder.userlist,
    access: accessMap.nationalSocietyUsers.list,
    icon: "Users",
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/add",
    title: () => strings(stringKeys.nationalSocietyUser.form.creationTitle),
    access: accessMap.nationalSocietyUsers.add,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/addExisting",
    title: () => strings(stringKeys.nationalSocietyUser.form.addExistingTitle),
    access: accessMap.nationalSocietyUsers.add,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit",
    title: () => strings(stringKeys.nationalSocietyUser.form.editionTitle),
    access: accessMap.nationalSocietyUsers.edit,
  },
];
