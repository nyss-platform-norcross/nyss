import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { placeholders } from "../../../siteMapPlaceholders";

export const translationsSiteMap = [
  {
    path: "/translations",
    title: () => strings(stringKeys.translations.title),
    access: accessMap.translations.list,
    placeholder: placeholders.tabMenu,
    placeholderIndex: 1
  },
  {
    path: "/emailTranslations",
    title: () => strings(stringKeys.translations.emailTitle),
    access: accessMap.translations.list,
    placeholder: placeholders.tabMenu,
    placeholderIndex: 2
  },
  {
    path: "/smsTranslations",
    title: () => strings(stringKeys.translations.smsTitle),
    access: accessMap.translations.list,
    placeholder: placeholders.tabMenu,
    placeholderIndex: 3
  }
];
