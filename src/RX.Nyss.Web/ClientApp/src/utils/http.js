import { stringKeys, stringKey } from "../strings";
import * as cache from "./cache";
import { reloadPage } from "./page";

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

export const ensureResponseIsSuccess = (response) => {
  if (!response.isSuccess) {
    throw new Error(stringKey((response.message && response.message.key) || stringKeys.error.responseNotSuccessful));
  }
};

const callApi = (path, method, data, headers = {}, authenticate = false) => {
  return new Promise((resolve, reject) => {
    let init = {
      method,
      redirect: "manual",
      headers: new Headers({
        ...headers,
        "Pragma": "no-cache",
        "Cache-Control": "no-cache",
        "Expires": "0"
      })
    };
    if (data) {
      init.body = JSON.stringify(data);
    }
    fetch(path, init)
      .then(response => {
        if (response.ok) {
          if (response.status === 204) {
            resolve();
          } else {
            resolve(response.json());
          }
        } else if (response.status === 0 && response.type === "opaqueredirect") {
          reject(new Error(stringKey(stringKeys.error.redirected)));
        } else if (response.status === 401) {
          reloadPage();
          reject(new Error(stringKey(stringKeys.error.notAuthenticated)));
        } else if (response.status === 403) {
          reject(new Error(stringKey(stringKeys.error.unauthorized)));
        } else {
          return response.json()
            .then(data => reject(new Error(stringKey(data.message.key))))
            .catch(e => reject(e));
        }
      });
  });
}
