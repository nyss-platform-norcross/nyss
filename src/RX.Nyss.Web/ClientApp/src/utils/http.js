import { getOrRenewToken } from "../authentication/auth";
import { strings } from "../strings";
import * as cache from "./cache";

export const post = (path, data, anonymous) => {
  const headers = {
    "Accept": "application/json",
    "Content-Type": "application/json"
  };
  return callApi(path, "POST", data || {}, headers, !anonymous)
    .then(response => {
      ensureResponseIsSuccess(response);
      return response;
    });
}

export const get = (path, anonymous) => {
  return callApi(path, "GET", undefined, {}, !anonymous)
    .then(response => {
      ensureResponseIsSuccess(response);
      return response;
    });
}

export const getCached = ({ path, dependencies }) =>
  cache.retrieve({
    key: path,
    setter: () => get(path),
    dependencies: dependencies
  });

export const ensureResponseIsSuccess = (response, message) => {
  if (!response.isSuccess) {
    throw new Error(message || getResponseErrorMessage(response.message));
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
          return response.json().then(data => reject(new Error(strings(data.message.key))));
        }
      });
  });
}

const getResponseErrorMessage = (message) =>
  (message && message.key)
    ? strings(message.key)
    : "";
