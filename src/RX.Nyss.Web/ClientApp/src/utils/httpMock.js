export const post = (path, dto) =>
  endpoints[path](dto);

export const get = (path, dto) =>
  endpoints[path](dto);

const endpoints = {
  "/api/nationalSocieties/get": () => delaySuccessCall(2000, [
    { id: 1, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
    { id: 2, name: "Sierra Leone Red Cross Society", country: "Sierra Leone", startDate: "2018-10-10T00:00:00Z", dataOwner: "John", technicalAdvisor: "Karine" },
  ]),

  "/api/nationalSocieties/create": (dto) => delaySuccessCall(2000),
  "/api/nationalSocieties/1/remove": (dto) => delaySuccessCall(2000),
};

const success = (value) => ({
  isSuccess: true,
  value: value
});

const delaySuccessCall = (time, data) => new Promise((resolve, reject) => {
  setTimeout(() => resolve(success(data)), time);
});