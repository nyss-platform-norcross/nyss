let cache = {};

export const retrieve = ({ key, setter, dependencies }) => {
  if (cache[key]) {
    return Promise.resolve(cache[key].value);
  }

  return setter().then((value) => {
    if (!value) {
      return null;
    }

    set(key, value, dependencies);

    return value;
  });
};

export const get = (key) => (cache[key] ? cache[key].value : null);

export const set = (key, value, dependencies) =>
  (cache[key] = {
    value: value,
    dependencies: dependencies,
  });

export const cleanCacheForDependencies = (dependencies) => {
  Object.keys(cache).forEach((key) => {
    const entry = cache[key];
    if (
      entry.dependencies &&
      entry.dependencies.some(
        (entryDep) => dependencies.indexOf(entryDep) !== -1,
      )
    ) {
      delete cache[key];
    }
  });
};
