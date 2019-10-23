let stringList = {
  "login.signIn": "Log innnn",
  "login.password": "Password",
  "login.forgotPassword": "Forgot password?",
  "login.notSucceeded": "Invalid user name or password"
};

export const stringKeys = {
  login: {
    signIn: "login.signIn",
    password: "login.password",
    forgotPassword: "login.forgotPassword",
    notSucceeded: "login.notSucceeded"
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
