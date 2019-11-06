import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";

export const nationalSocietyUsersSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/users",
    title: "Users",
    placeholder: placeholders.leftMenu,
    placeholderIndex: 2,
    access: accessMap.nationalSocietyUsers.list
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/add",
    title: "Add User",
    access: accessMap.nationalSocietyUsers.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/users",
    path: "/nationalsocieties/:nationalSocietyId/users/:nationalSocietyUserId/edit",
    title: "Edit User",
    access: accessMap.nationalSocietyUsers.edit
  }
];
