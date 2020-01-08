import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectLeftMenuOrder } from "../../projects/logic/projectsSiteMap";

export const dataCollectorsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/datacollectors",
    title: () => strings(stringKeys.dataCollector.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: projectLeftMenuOrder.dataCollectors
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/list",
    title: () => strings(stringKeys.dataCollector.list.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 1,
    middleStepOnly: true
  },
  {
    parentPath: "/projects/:projectId/datacollectors/list",
    path: "/projects/:projectId/datacollectors/add",
    title: () => strings(stringKeys.dataCollector.form.creationTitle),
    access: accessMap.dataCollectors.add
  },
  {
    parentPath: "/projects/:projectId/datacollectors/list",
    path: "/projects/:projectId/datacollectors/:dataCollectorId/edit",
    title: () => strings(stringKeys.dataCollector.form.editionTitle),
    access: accessMap.dataCollectors.edit
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/mapoverview",
    title: () => strings(stringKeys.dataCollector.mapOverview.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 2,
    middleStepOnly: true
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/performance",
    title: () => strings(stringKeys.dataCollector.performanceList.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.dataCollectors.performanceList,
    placeholderIndex: 3,
    middleStepOnly: true
  }
];
