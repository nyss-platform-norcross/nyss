import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";

export const nationalSocietyReportsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/reports",
    title: () => strings(stringKeys.reports.nationalSocietyReportsTitle),
    placeholder: placeholders.leftMenu,
    access: accessMap.nationalSocietyReports.list,
    placeholderIndex: nationalSocietyLeftMenuOrder.nationalSocietyReports,
    icon: "Report",
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/reports",
    path: "/nationalsocieties/:nationalSocietyId/reports/correct",
    title: () => strings(stringKeys.reports.correctReportsTitle),
    placeholder: placeholders.tabMenu,
    access: accessMap.nationalSocietyReports.list,
    placeholderIndex: 1,
    middleStepOnly: true,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/reports",
    path: "/nationalsocieties/:nationalSocietyId/reports/incorrect",
    title: () => strings(stringKeys.reports.incorrectReportsTitle),
    placeholder: placeholders.tabMenu,
    access: accessMap.nationalSocietyReports.list,
    placeholderIndex: 1,
    middleStepOnly: true,
  },
];
