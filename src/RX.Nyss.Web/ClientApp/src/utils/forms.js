import dayjs from "dayjs";
import {
  strings,
  stringKeys,
  stringsFormat,
  extractString,
  isStringKey,
} from "../strings";
import { useEffect } from "react";
import { reportAges } from "../components/reports/logic/reportsConstants";

const validateField = (field, formValues) => {
  if (field._validators && field._validators.length !== 0) {
    for (let validator of field._validators) {
      if (validator.length !== 2) {
        throw new Error("Wrong validator structure");
      }

      const isValid = validator[1](field.value, formValues);

      if (!isValid) {
        return {
          isValid: isValid,
          errorMessage: !isValid ? validator[0]() : null,
        };
      }
    }
  }
  if (field._customError) {
    return {
      isValid: false,
      errorMessage: field._customError,
    };
  }

  return {
    isValid: true,
    errorMessage: null,
  };
};

const revalidateField = (field, form) => {
  field.error = validateField(field, getFormValues(form)).errorMessage;
};

const onChange = (
  name,
  newValue,
  subscribers,
  form,
  suspendValidation,
  formSubscribers,
) => {
  const field = form[name];
  field.value = newValue;

  if (field._validators && !suspendValidation) {
    field.error = validateField(field, getFormValues(form)).errorMessage;
  }

  for (let subscriber of subscribers) {
    subscriber({ newValue: newValue, field: field, form: form });
  }

  for (let subscriber of formSubscribers) {
    subscriber();
  }
};

const validateFields = (fields) => {
  let isValid = true;
  for (let name in fields) {
    if (!fields.hasOwnProperty(name)) {
      continue;
    }

    const field = fields[name];
    field.update(field.value);

    if (field.error) {
      isValid = false;
    }
  }

  return isValid;
};

const getFormValues = (fields) => {
  let result = {};
  for (let name in fields) {
    if (!fields.hasOwnProperty(name)) {
      continue;
    }

    result[name] = fields[name].value;
  }
  return result;
};

const createFormField = (
  name,
  value,
  validatorDefinition,
  form,
  formSubscribers,
  ref,
) => {
  const subscribers = [];

  const field = {
    name: name,
    value: value,
    error: null,
    touched: false,
    ref: ref,
    subscribe: (callback) => {
      subscribers.push(callback);
      return {
        unsubscribe: () => subscribers.splice(subscribers.indexOf(callback), 1),
      };
    },
    _subscribers: subscribers,
    _validators: validatorDefinition,
    _customError: null,
    update: (newValue, suspendValidation) =>
      onChange(
        name,
        newValue,
        subscribers,
        form,
        suspendValidation,
        formSubscribers,
      ),
    scrollTo: () => ref.current && ref.current.scrollIntoView(),
  };

  field.setValidators = (newValidators) => (field._validators = newValidators);

  return field;
};

export const useCustomErrors = (form, error) =>
  useEffect(() => {
    form && form.setCustomErrors(error && error.data);
  }, [form, error]);

export const createForm = (fields, validators, refs) => {
  const form = {};

  const formSubscribers = [];

  for (let name in fields) {
    if (!fields.hasOwnProperty(name)) {
      continue;
    }

    form[name] = createFormField(
      name,
      fields[name],
      validators && validators[name],
      form,
      formSubscribers,
      refs && refs[name],
    );
  }

  const subscribeToForm = (callback) => {
    formSubscribers.push(callback);
    return {
      unsubscribe: () =>
        formSubscribers.splice(formSubscribers.indexOf(callback), 1),
    };
  };

  const removeField = (name) => {
    delete form[name];
  };

  const setCustomErrors = (errors) => {
    const customErrorsKeys = errors ? Object.keys(errors) : [];
    let hasNewState = false;

    for (let name in form) {
      if (!form.hasOwnProperty(name)) {
        continue;
      }

      const field = form[name];
      const customErrorKey = customErrorsKeys.find((e) =>
        e.indexOf("[") > -1
          ? e.replace("[", "_").replace("].", "_").toLowerCase() ===
            name.toLowerCase()
          : e.toLowerCase() === name.toLowerCase(),
      );

      if (customErrorKey) {
        const errorMessages = errors[customErrorKey];

        if (errorMessages && errorMessages.length) {
          if (!field._customError) {
            hasNewState = true;
          }

          field._customError = isStringKey(errorMessages[0])
            ? extractString(errorMessages[0])
            : errorMessages[0];
        }
      } else if (field._customError) {
        field._customError = null;
        hasNewState = true;
      }
    }

    hasNewState && validateFields(form);
  };

  return {
    fields: form,
    isValid: () => {
      setCustomErrors([]);
      return validateFields(form);
    },
    getValues: () => getFormValues(form),
    subscribe: subscribeToForm,
    addField: (name, value, fieldValidators, ref) => {
      form[name] = createFormField(
        name,
        value,
        fieldValidators,
        form,
        formSubscribers,
        ref,
      );
    },
    setCustomErrors: setCustomErrors,
    clearCustomErrors: () => setCustomErrors([]),
    removeField: (name, _, __) => removeField(name),
    subscribeOnce: (callback) => {
      const { unsubscribe } = subscribeToForm(() => {
        callback();
        unsubscribe();
      });
    },
    revalidateField: (field, formValues) => revalidateField(field, formValues),
  };
};

function timeNotInFuture(value) {
  const valueDate = dayjs(`${dayjs().format("YYYY-MM-DD")} ${value}`);
  return dayjs().isAfter(valueDate);
}

const emailRegex =
  /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
const phoneNumberRegex = /^\+[0-9]{6}[0-9]*$/;
const timeRegex = /^[0-9]{2}:[0-9]{2}/;

export const validators = {
  phoneNumber: [
    () => strings(stringKeys.validation.invalidPhoneNumber),
    (value) => !value || phoneNumberRegex.test(value),
  ],
  required: [
    () => strings(stringKeys.validation.fieldRequired),
    (value) => !!value,
  ],
  requiredWhen: (fieldGetter) => [
    () => "Value is required",
    (value, fields) => !fieldGetter(fields) || !!value,
  ],
  integer: [
    () => strings(stringKeys.validation.invalidInteger),
    (value) => !value || !isNaN(Number(value)),
  ],
  minLength: (minLength) => [
    () =>
      stringsFormat(stringKeys.validation.tooShortString, { value: minLength }),
    (value) => !value || value.length >= minLength,
  ],
  maxLength: (maxLength) => [
    () =>
      stringsFormat(stringKeys.validation.tooLongString, { value: maxLength }),
    (value) => !value || value.length <= maxLength,
  ],
  email: [
    () => strings(stringKeys.validation.invalidEmail),
    (value) => emailRegex.test(value),
  ],
  emailWhen: (fieldGetter) => [
    () => strings(stringKeys.validation.invalidEmail),
    (value, fields) => !fieldGetter(fields) || emailRegex.test(value),
  ],
  moduloTen: [
    () => strings(stringKeys.validation.invalidModuloTen),
    (value) => Number(value) % 10 === 0,
  ],
  nonNegativeNumber: [
    () => strings(stringKeys.validation.valueCannotBeNegative),
    (value) => !value || (!isNaN(Number(value)) && Number(value) >= 0),
  ],
  inRange: (min, max) => [
    () => stringsFormat(stringKeys.validation.inRange, { min, max }),
    (value) =>
      !value || (!isNaN(Number(value)) && value >= min && value <= max),
  ],
  time: [
    () => strings(stringKeys.validation.invalidTimeFormat),
    (value) => !value || timeRegex.test(value),
  ],
  sexAge: (fieldGetter) => [
    () => strings(stringKeys.validation.sexOrAgeUnspecified),
    (value, fields) =>
      fieldGetter(fields) === value || value !== reportAges.unspecified,
  ],
  uniqueLocation: (fieldGetter, allLocations) => [
    () => strings(stringKeys.validation.duplicateLocation),
    (value, fields) =>
      allLocations.length === 1 ||
      allLocations.filter(
        (l) =>
          l.villageId.toString() === value &&
          (l.zoneId == null || l.zoneId.toString() === fieldGetter(fields)),
      ).length <= 1,
  ],
  timeNotInFuture: [
    () => strings(stringKeys.validation.timeNotInFuture),
    timeNotInFuture,
  ],
};
