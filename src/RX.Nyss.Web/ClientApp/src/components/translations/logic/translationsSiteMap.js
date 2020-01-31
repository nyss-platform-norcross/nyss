import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";

export const translationsSiteMap = [
  {
    parentPath: "/",
    path: "/translations",
    title: () => strings(stringKeys.translations.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.translations.list
  }
];
