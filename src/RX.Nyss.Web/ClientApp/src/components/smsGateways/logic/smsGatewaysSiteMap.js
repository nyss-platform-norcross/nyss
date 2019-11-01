import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";

export const smsGatewaysSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways",
    title: "SMS Gateways",
    placeholder: placeholders.topMenu,
    access: accessMap.smsGateways.list
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/add",
    title: "Add SMS Gateway",
    access: accessMap.smsGateways.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/smsgateways",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways/:smsGatewayId/edit",
    title: "Edit SMS Gateway",
    access: accessMap.smsGateways.edit
  }
];
