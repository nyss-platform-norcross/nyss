import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";

export const healthRisksSiteMap = [
  {
    path: "/healthrisks",
    title: "Health Risks / Events",
    placeholder: placeholders.topMenu,
    access: accessMap.healthRisks.list
  },
  {
    parentPath: "/healthrisks",
    path: "/healthrisks/add",
    title: "Add Health Risk / Event",
    access: accessMap.healthRisks.add
  },
  {
    parentPath: "/healthrisks",
    path: "/healthrisks/:healthRiskId/edit",
    title: "Edit heath risk/event: {healthRiskCode}",
    access: accessMap.healthRisks.edit
  }
];
