import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const healthRisksSiteMap = [
  {
    path: "/healthrisks",
    title: () => strings(stringKeys.healthRisk.title),
    placeholder: placeholders.topMenu,
    access: accessMap.healthRisks.list
  },
  {
    parentPath: "/healthrisks",
    path: "/healthrisks/add",
    title: () => strings(stringKeys.healthRisk.form.creationTitle),
    access: accessMap.healthRisks.add
  },
  {
    parentPath: "/healthrisks",
    path: "/healthrisks/:healthRiskId/edit",
    title: () => strings(stringKeys.healthRisk.form.editionTitle),
    access: accessMap.healthRisks.edit
  }
];
