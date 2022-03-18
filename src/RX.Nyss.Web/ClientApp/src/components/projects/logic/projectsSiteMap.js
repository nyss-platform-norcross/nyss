import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";

export const projectLeftMenuOrder = {
  dashboard: 0,
  alerts: 10,
  dataCollectors: 20,
  reports: 30,
  settings: 40
};

export const projectsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: () => strings(stringKeys.project.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.projects.list,
    placeholderIndex: nationalSocietyLeftMenuOrder.projects
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/add",
    title: () => strings(stringKeys.project.form.creationTitle),
    access: accessMap.projects.add
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    title: () => "{projectName}",
    access: accessMap.projects.get
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    title: () => strings(stringKeys.project.settingsRootTitle),
    placeholder: placeholders.leftMenu,
    access: accessMap.projects.showOverview,
    placeholderIndex: projectLeftMenuOrder.settings
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/overview",
    title: () => strings(stringKeys.project.settings),
    access: accessMap.projects.showOverview,
    placeholder: placeholders.tabMenu,
    placeholderIndex: 1,
    middleStepOnly: true
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/edit",
    title: () => strings(stringKeys.project.form.editionTitle),
    access: accessMap.projects.edit,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/alertNotifications",
    title: () => strings(stringKeys.projectAlertRecipient.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.projectAlertNotifications.list,
    placeholderIndex: 3,
    middleStepOnly: true
  },
  {
    parentPath: "/projects/:projectId/alertNotifications",
    path: "/projects/:projectId/alertNotifications/addRecipient",
    title: () => strings(stringKeys.projectAlertRecipient.form.creationTitle),
    access: accessMap.projectAlertNotifications.addRecipient
  },
  {
    parentPath: "/projects/:projectId/alertNotifications",
    path: "/projects/:projectId/alertNotifications/:alertRecipientId/editRecipient",
    title: () => strings(stringKeys.projectAlertRecipient.form.editionTitle),
    access: accessMap.projectAlertNotifications.editRecipient
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/errorMessages",
    title: () => strings(stringKeys.project.errorMessages.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.projectErrorMessages.list,
    placeholderIndex: 4,
    middleStepOnly: true,
  },
];
