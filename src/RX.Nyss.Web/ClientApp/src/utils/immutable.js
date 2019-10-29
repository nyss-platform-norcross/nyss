export const removeProperty = (obj, key) => {
  const { [key]: _, ...rest } = obj;
  return rest;
};

export const setProperty = (obj, key, value) =>
  value === undefined
    ? removeProperty(obj, key)
    : {
      ...obj,
      [key]: value,
    }