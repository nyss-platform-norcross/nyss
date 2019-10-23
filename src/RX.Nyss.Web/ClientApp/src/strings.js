let stringList = {
  "login.signIn": "Log innnn",
  "login.password": "Password",
  "login.forgotPassword": "Forgot password?"
};

export const stringKeys = {
  login: {
    signIn: "login.signIn",
    password: "login.password",
    forgotPassword: "login.forgotPassword"
  }
};

export const strings = (key) => {
  const value = stringList[key];
  return value === undefined ? "String not defined" : value;
}

export function updateStrings(strings) {
  Object.assign(stringList, strings);
}

export default stringList;
