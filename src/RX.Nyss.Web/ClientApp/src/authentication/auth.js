import * as localStorage from "../utils/localStorage";
import jwt_decode from "jwt-decode";

const localStorageKeys = {
  accessToken: "accessToken",
  redirectUrl: "redirect-url"
}

let cachedUser = null;

export const rootUrl = "/";

export const loginUrl = "/login";

export const redirectToLogin = () => redirectTo(loginUrl);

export const redirectToRoot = () => redirectTo("/");

export const redirectTo = (path) => window.location.pathname = path;

export const isAccessTokenSet = () => !!localStorage.get(localStorageKeys.accessToken);

export const setAccessToken = (token) => localStorage.set(localStorageKeys.accessToken, token);

export const getAccessToken = () => localStorage.get(localStorageKeys.accessToken);

export const getAccessTokenData = () => {
  if (cachedUser) {
    return cachedUser;
  }

  const accessToken = getAccessToken();

  if (!accessToken) {
    return null;
  }

  var tokenData = jwt_decode(accessToken);

  if (!tokenData) {
    throw new Error("Unexpected token data structure");
  }

  cachedUser = {
    name: tokenData["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
    role: tokenData["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
  }

  return cachedUser;
};

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