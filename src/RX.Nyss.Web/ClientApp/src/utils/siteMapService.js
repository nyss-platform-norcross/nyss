import { siteMap } from "../siteMap";
import { getAccessTokenData } from "../authentication/auth";

export const getMenu = (path, siteMapParameters, placeholder, currentPath) => {
  if (!path) {
    return [];
  }

  const breadcrumb = currentPath ? getBreadcrumb(currentPath, siteMapParameters) : [];

  let menuPath = path;

  if (path !== "/") {
    for (let i = breadcrumb.length - 1; i >= 0; i--) {
      if (breadcrumb[i].placeholder === placeholder) {
        menuPath = breadcrumb[i].parentPath;
        break;
      }
    }
  }

  return siteMap
    .filter(item => item.parentPath === menuPath && item.placeholder && item.placeholder === placeholder)
    .map(item => ({
      title: item.title,
      url: getUrl(item.path, siteMapParameters),
      isActive: breadcrumb.some(b => b.path === item.path)
    }))
};

export const getBreadcrumb = (path, siteMapParameters) => {
  const authUser = getAccessTokenData();

  if (!authUser) {
    return [];
  }

  if (path === "/") {
    return [];
  }

  const role = authUser.role;
  const mapItem = findSiteMapItem(path);

  let currentItem = mapItem;
  let hierarchy = [];

  while (true) {
    if (!currentItem.access || !currentItem.access.length || (currentItem.access.some(item => item === role))) {
      hierarchy.splice(0, 0, {
        path: currentItem.path,
        title: getTitle(currentItem.title, siteMapParameters),
        url: getUrl(currentItem.path, siteMapParameters),
        isActive: currentItem.path === path,
        placeholder: currentItem.placeholder,
        parentPath: currentItem.parentPath
      });
    }

    if (currentItem.parentPath === "/") {
      break;
    }

    currentItem = findSiteMapItem(currentItem.parentPath);
  }

  return hierarchy;
}

const getTitle = (template, params) => {
  let result = template;
  for (let key in params) {
    result = result.replace(`{${key}}`, params[key])
  }
  return result;
}

const getUrl = (template, params) => {
  let result = template;
  for (let key in params) {
    result = result.replace(`:${key}`, params[key])
  }
  return result;
}

const findSiteMapItem = (path) => {
  const item = siteMap.find(item => item.path === path);
  if (!item) {
    throw new Error(`SiteMap configuration is inconsistent. Cannot find item with path: ${path}`)
  }
  return item;
}