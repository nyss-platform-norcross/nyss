import { strings, stringKeys, stringsFormat } from "../strings";

const validateField = (field, validators, formValues) => {
  if (validators && validators.length !== 0) {
    for (let validator of validators) {
      if (validator.length !== 2) {
        throw new Error("Wrong validator structure");
      }

      const isValid = validator[1](field.value, formValues);

      if (!isValid) {
        return {
          isValid: isValid,
          errorMessage: !isValid ? validator[0]() : null
        };
      }
    }
  }

  return {
    isValid: true,
    errorMessage: null
  };
};

const onChange = (name, newValue, subscribers, form, validatorDefinition, suspendValidation, formSubscribers) => {
  const field = form[name];
  field.value = newValue;

  if (validatorDefinition && !suspendValidation) {
    field.error = validateField(field, validatorDefinition, getFormValues(form)).errorMessage;
  }

  for (let subscriber of subscribers) {
    subscriber({ newValue: newValue, field: field, form: form });
  }

  for (let subscriber of formSubscribers) {
    subscriber();
  }
};

const validateFields = (form, validators) => {
  let isValid = true;
  const formValues = getFormValues(form);

  for (let name in validators) {
    if (!validators.hasOwnProperty(name)) {
      continue;
    }

    const validationResult = validateField(form[name], validators[name], formValues);

    form[name].error = validationResult.errorMessage;
    form[name].update(form[name].value);

    if (!validationResult.isValid) {
      isValid = false;
    }
  }

  return isValid;
};

const getFormValues = (form) => {
  let result = {};
  for (let name in form) {
    if (!form.hasOwnProperty(name)) {
      continue;
    }

    result[name] = form[name].value;
  }
  return result;
};

const createFormField = (name, value, validatorDefinition, form, formSubscribers) => {
  const subscribers = [];

  return {
    name: name,
    value: value,
    error: null,
    touched: false,
    subscribe: callback => {
      subscribers.push(callback);
      return {
        unsubscribe: () => subscribers.splice(subscribers.indexOf(callback), 1)
      };
    },
    _subscribers: subscribers,
    update: (newValue, suspendValidation) => onChange(name, newValue, subscribers, form, validatorDefinition, suspendValidation, formSubscribers)
  }
}

export const createForm = (fields, validators) => {
  const form = {};

  const formSubscribers = [];

  for (let name in fields) {
    if (!fields.hasOwnProperty(name)) {
      continue;
    }

    form[name] = createFormField(name, fields[name], validators && validators[name], form, formSubscribers);
  }

  const subscribeToForm = callback => {
    formSubscribers.push(callback);
    return {
      unsubscribe: () => formSubscribers.splice(formSubscribers.indexOf(callback), 1)
    };
  };

  const removeField = (name) => {
    delete form[name];
  };

  return {
    fields: form,
    isValid: () => validateFields(form, validators, getFormValues(form)),
    getValues: () => getFormValues(form),
    subscribe: subscribeToForm,
    addField: (name, value, validators) => { form[name] = createFormField(name, value, validators, form, formSubscribers) },
    removeField: (name, value, validators) => removeField(name),
    subscribeOnce: callback => {
      const { unsubscribe } = subscribeToForm(() => {
        callback();
        unsubscribe();
      });
    }
  };
};

const emailRegex = /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
const phoneNumberRegex = /^\+[0-9]{6}[0-9]*$/;

export const validators = {
  phoneNumber: [() => strings(stringKeys.validation.invalidPhoneNumber), (value) => !value || phoneNumberRegex.test(value)],
  required: [() => strings(stringKeys.validation.fieldRequired), (value) => !!value],
  requiredWhen: (fieldGetter) => [() => "Value is required", (value, fields) => !fieldGetter(fields) || !!value],
  integer: [() => strings(stringKeys.validation.invalidInteger), (value) => !value || !isNaN(Number(value))],
  minLength: (minLength) => [() => stringsFormat(stringKeys.validation.tooShortString, { value: minLength }), (value) => !value || value.length >= minLength],
  maxLength: (maxLength) => [() => stringsFormat(stringKeys.validation.tooLongString, { value: maxLength }), (value) => !value || value.length <= maxLength],
  email: [() => strings(stringKeys.validation.invalidEmail), (value) => emailRegex.test(value)],
  emailWhen: (fieldGetter) => [() => strings(stringKeys.validation.invalidEmail), (value, fields) => !fieldGetter(fields) || emailRegex.test(value)],
  moduloTen: [() => strings(stringKeys.validation.invalidModuloTen), (value) => (Number(value) % 10 === 0)],
  nonNegativeNumber: [() => strings(stringKeys.validation.valueCannotBeNegative), (value) => !value || (!isNaN(Number(value)) && Number(value) >= 0)],
  inRange: (min, max) => [() => stringsFormat(stringKeys.validation.inRange, { min, max }), (value) => !value || (!isNaN(Number(value)) && value >= min && value <= max)]
};
