import { getOrRenewToken } from "../authentication/auth";

export const post = (path, data, anonymous) => {
  const headers = {
    "Accept": "application/json",
    "Content-Type": "application/json"
  };
  return callApi(path, "POST", data || {}, headers, !anonymous);
}

export const get = (path) => {
  return callApi(path, "GET", undefined, {}, true);
}

export const ensureResponseIsSuccess = (response, message) => {
  if (!response.isSuccess) {
    throw new Error((message || "An error has occured") + ". " + getResponseErrorMessage(response.error));
  }
};

const callApi = (path, method, data, headers = {}, authenticate = false) => {
  return new Promise((resolve, reject) => {
    let authentication = authenticate ? getOrRenewToken() : new Promise(r => r());

    authentication
      .then(token => {
        const fetchHeaders = authenticate ? { ...headers, "Authorization": "Bearer " + token } : headers;
        let init = { method, headers: new Headers(fetchHeaders) };
        if (data) {
          init.body = JSON.stringify(data);
        }
        return fetch(path, init);
      })
      .then(response => {
        if (response.ok) {
          if (response.status === 204) {
            resolve();
          } else {
            resolve(response.json());
          }
        } else if (response.status === 401) {
          // logout
          reject(new Error("UNAUTHORIZED"));
        } else {
          reject(new Error(`${path}: ${response.statusText}`));
        }
      });
  });
}

const getResponseErrorMessage = (error) =>
  (error && (error.message || error.key))
    ? (error.message || error.key).substring(0, 150)
    : "";
