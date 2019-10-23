import * as localStorage from "../utils/localStorage";

const localStorageKeys = {
  accessToken: "accessToken",
  redirectUrl: "redirect-url"
}

export const rootUrl = "/";

export const loginUrl = "/login";

export const redirectToLogin = () => redirectTo(loginUrl);

export const redirectToRoot = () => redirectTo("/");

export const redirectTo = (path) => window.location.pathname = path;

export const isAccessTokenSet = () => !!localStorage.get(localStorageKeys.accessToken);

export const setAccessToken = (token) => localStorage.set(localStorageKeys.accessToken, token);

export const getAccessToken = () => localStorage.get(localStorageKeys.accessToken);

export const removeAccessToken = () => localStorage.remove(localStorageKeys.accessToken);

export const getRedirectUrl = () => localStorage.get(localStorageKeys.redirectUrl);

export const setRedirectUrl = (url) => localStorage.set(localStorageKeys.redirectUrl, url);

export const removeRedirectUrl = (url) => localStorage.remove(localStorageKeys.redirectUrl);

export const getOrRenewToken = () => new Promise((resolve, reject) => {
  const token = getAccessToken();

  if (token) {
    return resolve(token);
  }

  return reject("User not logged in!");
});