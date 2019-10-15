export const action = (name) => ({
    INVOKE: `${name}_INVOKE`,
    REQUEST: `${name}_REQUEST`,
    SUCCESS: `${name}_SUCCESS`,
    FAILURE: `${name}_FAILURE`,
    getId: (id) => `${name}_${id}`
});
