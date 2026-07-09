import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { nationalSocietyLeftMenuOrder } from "../../nationalSocieties/logic/nationalSocietiesSiteMap";

export const projectTabMenuOrder = {
  dashboard: 0,
  alerts: 10,
  reports: 20,
  dataCollectors: 30,
  settings: 40,
};

const projectSubMenuOrder = {
  general: 0,
  healthRisks: 1,
  unhandledAlertRecipients: 2,
  escalatedAlertRecipients: 3,
  errorMessages: 4,
};

export const projectsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId",
    path: "/nationalsocieties/:nationalSocietyId/projects",
    title: () => strings(stringKeys.project.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.projects.list,
    placeholderIndex: nationalSocietyLeftMenuOrder.projects,
    icon: "Project",
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/add",
    title: () => strings(stringKeys.project.form.creationTitle),
    access: accessMap.projects.add,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    title: () => "{projectName}",
    access: accessMap.projects.get,
  },
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    title: () => strings(stringKeys.project.settingsRootTitle),
    placeholder: placeholders.projectTabMenu,
    access: accessMap.projects.settings,
    placeholderIndex: projectTabMenuOrder.settings,
    icon: "Settings",
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/overview",
    title: () => strings(stringKeys.project.settings),
    access: accessMap.projects.overview,
    placeholder: placeholders.projectSubMenu,
    placeholderIndex: 1,
    middleStepOnly: true,
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/edit",
    title: () => strings(stringKeys.project.form.editionTitle),
    access: accessMap.projects.edit,
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/healthrisks",
    title: () => strings(stringKeys.healthRisk.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.projects.projectHealthRisks,
    placeholderIndex: projectSubMenuOrder.healthRisks,
    middleStepOnly: true,
    icon: "HealthRisks"
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/nationalsocieties/:nationalSocietyId/projects/:projectId/editHealthRisks",
    title: () => strings(stringKeys.project.form.healthRisksEditionTitle),
    access: accessMap.projects.edit,
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/escalatedAlertNotifications",
    title: () => strings(stringKeys.projectAlertRecipient.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.projectAlertNotifications.list,
    placeholderIndex: projectSubMenuOrder.escalatedAlertRecipients,
    middleStepOnly: true,
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/unhandledAlertNotifications",
    title: () => strings(stringKeys.projectAlertNotHandledRecipient.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.projectAlertNotifications.list,
    placeholderIndex: projectSubMenuOrder.unhandledAlertRecipients,
    middleStepOnly: true,
  },
  {
    parentPath: "/projects/:projectId/escalatedAlertNotifications",
    path: "/projects/:projectId/escalatedAlertNotifications/addRecipient",
    title: () => strings(stringKeys.projectAlertRecipient.form.creationTitle),
    access: accessMap.projectAlertNotifications.addRecipient,
  },
  {
    parentPath: "/projects/:projectId/escalatedAlertNotifications",
    path: "/projects/:projectId/escalatedAlertNotifications/:alertRecipientId/editRecipient",
    title: () => strings(stringKeys.projectAlertRecipient.form.editionTitle),
    access: accessMap.projectAlertNotifications.editRecipient,
  },
  {
    parentPath:
      "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/errorMessages",
    title: () => strings(stringKeys.project.errorMessages.title),
    placeholder: placeholders.projectSubMenu,
    access: accessMap.projectErrorMessages.list,
    placeholderIndex: 4,
    middleStepOnly: true,
  },
];
