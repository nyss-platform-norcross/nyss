import {stringKeys, strings} from "../../../strings";
import {accessMap} from "../../../authentication/accessMap";
import {placeholders} from "../../../siteMapPlaceholders";

export const alertEventsSiteMap = [
  {
    parentPath: "/projects/:projectId/alerts/:alertId/details",
    path: "/projects/:projectId/alerts/:alertId/eventLog",
    title: () => strings(stringKeys.alerts.logs.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.alertEvents.list,
    middleStepOnly: true
  }
]