export const post = (path, dto) =>
  endpoints[path](dto);

export const get = (path, dto) =>
  endpoints[path](dto);

const endpoints = {
  "/api/healthrisk/list": () => delaySuccessCall(2000, [
    { id: 1, number: 1, name: "Cholera", type: "Human" },
    { id: 2, number: 5, name: "Measles", type: "Human" },
  ]),

  "/api/nationalSocieties/1/get": () => delaySuccessCall(1000, {
    id: 1, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", countryId: 1, contentLanguageId: 1
  }),

  "/api/nationalSocieties/create": (dto) => delaySuccessCall(2000),

  "/api/nationalSocieties/1/edit": (dto) => delaySuccessCall(2000),

  "/api/nationalSocieties/1/remove": (dto) => delaySuccessCall(2000),
};

const success = (value) => ({
  isSuccess: true,
  value: value
});

const delaySuccessCall = (time, data) => new Promise((resolve, reject) => {
  setTimeout(() => resolve(success(data)), time);
});