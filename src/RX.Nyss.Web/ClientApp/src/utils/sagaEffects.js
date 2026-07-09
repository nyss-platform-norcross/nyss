import { call } from "redux-saga/effects";

export const autoRestart = (generator) => {
  return function* autoRestarting(...args) {
    while (true) {
      try {
        yield call(generator, ...args);
      } catch (e) {
        console.error(`Unhandled error in '${generator.name}'`, e);
      }
    }
  };
};
