import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const dataCollectorsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/datacollectors",
    title: () => strings(stringKeys.dataCollector.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 1
  },
  {
    parentPath: "/projects/:projectId/datacollectors",
    path: "/projects/:projectId/datacollectors/list",
    title: () => strings(stringKeys.dataCollector.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.dataCollectors.list,
    placeholderIndex: 1
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
    placeholderIndex: 2
  },
];
