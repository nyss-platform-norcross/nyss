import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const suspectedDiseaseSiteMap = [
  {
    path: "/suspecteddiseases",
    title: () => strings(stringKeys.suspectedDisease.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.suspectedDiseases.list,
    placeholderIndex: 2,
  },
  {
    parentPath: "/suspecteddiseases",
    path: "/suspecteddiseases/add",
    title: () => strings(stringKeys.suspectedDisease.form.creationTitle),
    access: accessMap.suspectedDiseases.add,
  },
  {
    parentPath: "/suspecteddiseases",
    path: "/suspecteddiseases/:suspecteddiseaseId/edit",
    title: () => strings(stringKeys.suspectedDisease.form.editionTitle),
    access: accessMap.suspectedDiseases.edit,
  },
];
