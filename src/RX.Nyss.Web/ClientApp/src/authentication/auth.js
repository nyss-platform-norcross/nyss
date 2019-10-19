import * as localStorage from "../utils/localStorage";

const localStorageKeys = {
    user: "user",
    redirectUrl: "redirect-url"
}

export const localStorageUserKey = "user";
export const localStorageRedirectUrlKey = "redirect-url";

export const isAuthorized = () => !!localStorage.get(localStorageKeys.user);

export const setAuthorizedFlag = (userName) => localStorage.set(localStorageKeys.user, userName);

export const removeAuthorizedFlag = () => localStorage.remove(localStorageKeys.user);

export const getRedirectUrl = () => localStorage.get(localStorageKeys.redirectUrl);

export const setRedirectUrl = (url) => localStorage.set(localStorageKeys.redirectUrl, url);

export const removeRedirectUrl = (url) => localStorage.remove(localStorageKeys.redirectUrl);
