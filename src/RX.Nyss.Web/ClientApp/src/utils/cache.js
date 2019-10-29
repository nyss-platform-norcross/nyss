let cache = {};

export const retrieve = (key, setter) => {
  if (cache[key]) {
    return cache[key];
  }

  const value = setter();

  if (!value) {
    return null;
  }

  cache[key] = value;
  return value;
};

export const get = (key) => cache[key];

export const set = (key, value) => cache[key] = value;