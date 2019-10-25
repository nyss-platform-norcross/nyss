let stringList = {
  "login.signIn": "Log innnn",
  "login.password": "Password",
  "login.forgotPassword": "Forgot password?",
  "login.notSucceeded": "Invalid user name or password",
  "login.lockedOut": "Your account has been locked for 5 minutes due to too many failed login attempts. Please reset your password or try again later."
};

export const stringKeys = {
  login: {
    signIn: "login.signIn",
    password: "login.password",
    forgotPassword: "login.forgotPassword",
    notSucceeded: "login.notSucceeded",
    lockedOut: "login.lockedOut"
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
