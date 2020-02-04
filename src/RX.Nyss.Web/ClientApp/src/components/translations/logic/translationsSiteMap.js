import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const translationsSiteMap = [
  {
    path: "/translations",
    title: () => strings(stringKeys.translations.title),
    access: accessMap.translations.list
  }
];
