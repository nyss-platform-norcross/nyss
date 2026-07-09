import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectTabMenuOrder } from "../../projects/logic/projectsSiteMap";

export const dataCollectorsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/datacollectors",
    title: () => strings(stringKeys.dataCollectors.title),
    placeholder: placeholders.projectTabMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: projectTabMenuOrder.dataCollectors,
    icon: "Users",
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/list",
    title: () => strings(stringKeys.dataCollectors.list.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 1,
    middleStepOnly: true,
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/add",
    title: () => strings(stringKeys.dataCollectors.form.creationTitle),
    access: accessMap.dataCollectors.add,
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/:dataCollectorId/edit",
    title: () => strings(stringKeys.dataCollectors.form.editionTitle),
    access: accessMap.dataCollectors.edit,
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/mapoverview",
    title: () => strings(stringKeys.dataCollectors.mapOverview.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 2,
    middleStepOnly: true,
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/performance",
    title: () => strings(stringKeys.dataCollectors.performanceList.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.dataCollectors.performanceList,
    placeholderIndex: 3,
    middleStepOnly: true,
  },
];
