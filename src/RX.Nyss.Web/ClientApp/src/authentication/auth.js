import * as localStorage from "../utils/localStorage";

const localStorageKeys = {
  redirectUrl: "redirect-url",
};

export const rootUrl = "/";

export const loginUrl = "/login";

export const redirectToLogin = () => redirectTo(loginUrl);

export const redirectToRoot = () => redirectTo("/");

export const redirectTo = (path) => (window.location.pathname = path);

export const getRedirectUrl = () =>
  localStorage.get(localStorageKeys.redirectUrl);

export const setRedirectUrl = (url) =>
  localStorage.set(localStorageKeys.redirectUrl, url);

export const removeRedirectUrl = (url) =>
  localStorage.remove(localStorageKeys.redirectUrl);
