import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";

export const nationalSocietyDashboardSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/dashboard",
    title: () => strings(stringKeys.nationalSociety.dashboard.title),
    access: accessMap.nationalSocieties.showDashboard,
    placeholder: placeholders.leftMenu,
    placeholderIndex: nationalSocietyLeftMenuOrder.dashboard
  }
];
