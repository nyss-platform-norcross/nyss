import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const projectAlertRecipientsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId/settings",
    path: "/projects/:projectId/alertRecipients",
    title: () => strings(stringKeys.projectAlertRecipient.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.projectAlertRecipients.list,
    placeholderIndex: 2,
    middleStepOnly: true
  },
  {
    parentPath: "/projects/:projectId/alertRecipients",
    path: "/projects/:projectId/alertRecipients/add",
    title: () => strings(stringKeys.projectAlertRecipient.form.creationTitle),
    access: accessMap.projectAlertRecipients.add
  },
  {
    parentPath: "/projects/:projectId/alertRecipients",
    path: "/projects/:projectId/alertRecipients/:alertRecipientId/edit",
    title: () => strings(stringKeys.projectAlertRecipient.form.editionTitle),
    access: accessMap.projectAlertRecipients.edit
  }
];
