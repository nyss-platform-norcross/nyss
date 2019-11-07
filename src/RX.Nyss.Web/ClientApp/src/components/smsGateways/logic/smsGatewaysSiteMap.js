import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const smsGatewaysSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways",
    title: () => strings(stringKeys.smsGateway.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.smsGateways.list
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/add",
    title: () => strings(stringKeys.smsGateway.form.creationTitle),
    access: accessMap.smsGateways.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit",
    title: () => strings(stringKeys.smsGateway.form.editionTitle),
    access: accessMap.smsGateways.edit
  }
];
