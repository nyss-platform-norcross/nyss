export const post = (path, dto) => endpoints[path](dto);

export const get = (path, dto) => endpoints[path](dto);

const endpoints = {
  "/api/project/1/dataCollector/list": () =>
    delaySuccessCall(1000, [
      { id: 1, displayName: "Johny", name: "John", sex: "Male" },
      { id: 2, displayName: "Snowy", name: "Snow", sex: "Female" },
    ]),
};

const success = (value) => ({
  isSuccess: true,
  value: value,
});

const delaySuccessCall = (time, data) =>
  new Promise((resolve, reject) => {
    setTimeout(() => resolve(success(data)), time);
  });
