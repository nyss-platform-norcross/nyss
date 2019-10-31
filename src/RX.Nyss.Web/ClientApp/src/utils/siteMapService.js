import { siteMap } from "../siteMap";
import { getAccessTokenData } from "../authentication/auth";

const findClosestMenu = (breadcrumb, placeholder, pathForMenu) => {
  for (let i = breadcrumb.length - 1; i >= 0; i--) {
    if (breadcrumb[i].siteMapData.placeholder === placeholder) {
      return breadcrumb[i].siteMapData.parentPath;
    }
  }
};

export const getMenu = (pathForMenu, parameters, placeholder, currentPath) => {
  const breadcrumb = getBreadcrumb(currentPath, parameters);
  const closestMenuPath = findClosestMenu(breadcrumb, placeholder, pathForMenu);

  return siteMap
    .filter(item => item.parentPath === closestMenuPath && item.placeholder && item.placeholder === placeholder)
    .map(item => ({
      title: item.title,
      url: getUrl(item.path, parameters),
      isActive: breadcrumb.some(b => b.siteMapData.path === item.path)
    }))
};

export const getBreadcrumb = (path, siteMapParameters) => {
  const authUser = getAccessTokenData();

  if (!authUser || !path) {
    return [];
  }

  let currentItem = findSiteMapItem(path);
  let hierarchy = [];

  while (true) {
    if (!currentItem.access || !currentItem.access.length || currentItem.access.some(role => role === authUser.role)) {
      hierarchy.splice(0, 0, {
        title: getTitle(currentItem.title, siteMapParameters),
        url: getUrl(currentItem.path, siteMapParameters),
        isActive: currentItem.path === path,
        siteMapData: { ...currentItem }
      });
    }

    if (!currentItem.parentPath) {
      break;
    }

    currentItem = findSiteMapItem(currentItem.parentPath);
  }

  return hierarchy;
}

const getTitle = (template, params) =>
  Object.keys(params).reduce((result, key) => result.replace(`{${key}}`, params[key]), template);

const getUrl = (template, params) =>
  Object.keys(params).reduce((result, key) => result.replace(`:${key}`, params[key]), template);

const findSiteMapItem = (path) => {
  const item = siteMap.find(item => item.path === path);
  if (!item) {
    throw new Error(`SiteMap configuration is inconsistent. Cannot find item with path: ${path}`)
  }
  return item;
}