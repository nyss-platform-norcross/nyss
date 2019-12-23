import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";


export const nationalSocietyReportsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/reports",
    title: () => strings(stringKeys.nationalSocietyReports.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.nationalSocietyReports.list,
    placeholderIndex: nationalSocietyLeftMenuOrder.nationalSocietyReports
  }
];
