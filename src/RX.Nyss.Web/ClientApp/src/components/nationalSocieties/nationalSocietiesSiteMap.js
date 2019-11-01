import { placeholders } from "../../siteMapPlaceholders";
import { accessMap } from "../../authentication/accessMap";
import { Administrator, GlobalCoordinator, DataConsumer, DataManager } from "../../authentication/roles";

export const nationalSocietiesSiteMap = [
  {
    path: "/nationalsocieties",
    title: "National societies",
    placeholder: placeholders.topMenu,
    access: accessMap.nationalSocieties.list
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/add",
    title: "Add National Society",
    access: accessMap.nationalSocieties.add
  },
  {
    parentPath: "/nationalsocieties",
    path: "/nationalsocieties/:nationalSocietyId",
    title: "{nationalSocietyName} ({nationalSocietyCountry})",
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/dashboard",
    title: "Dashboard",
    access: [Administrator, GlobalCoordinator, DataConsumer],
    placeholder: placeholders.leftMenu,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/overview",
    path: "/nationalsocieties/:nationalSocietyId/edit",
    title: "Edit",
    access: [Administrator, GlobalCoordinator, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: "Projects",
    placeholder: placeholders.leftMenu,
    access: [Administrator, GlobalCoordinator, DataManager, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/overview",
    title: "Overview",
    placeholder: placeholders.leftMenu,
    access: [Administrator, GlobalCoordinator, DataManager, DataConsumer]
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/smsgateways",
    title: "SMS Gateways",
    placeholder: placeholders.leftMenu,
    access: accessMap.smsGateways.list
  }
];
