import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const nationalSocietyStructureSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/settings",
    path: "/nationalsocieties/:nationalSocietyId/structure",
    title: () => strings(stringKeys.nationalSociety.structure.title),
    placeholder: placeholders.tabMenu,
    placeholderIndex: 3,
    access: accessMap.nationalSocietyStructure.edit,
    middleStepOnly: true,
  },
];
