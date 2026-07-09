export const removeProperty = (obj, key) => {
  const { [key]: _, ...rest } = obj;
  return rest;
};

export const removeFromArray = (array, predicate) =>
  array.filter((item) => !predicate(item));

export const assignInArray = (array, predicate, changes) =>
  array.map((item) => (predicate(item) ? changes(item) : item));

export const setProperty = (obj, key, value) =>
  value === undefined
    ? removeProperty(obj, key)
    : {
        ...obj,
        [key]: value,
      };
