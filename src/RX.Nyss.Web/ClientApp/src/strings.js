let stringList = {
  "login.signIn": "Log innnn",
  "login.password": "Password",
  "login.forgotPassword": "Forgot password?",
  "login.notSucceeded": "Invalid user name or password",
  "validation.validationError": "There was a problem with validation",
  "login.lockedOut": "Your account has been locked for 5 minutes due to too many failed login attempts. Please reset your password or try again later.",
  "user.verifyEmail.setPassword": "Please set your password",
  "user.verifyEmail.welcome": "Welcome to Nyss",
  "user.verifyEmail.signIn": "Log in",
  "user.verifyEmail.password": "Password",
  "user.verifyEmail.failed": "Could not verify email address",
  "user.verifyEmail.addPassword.failed": "There was a problem with creating password",
  "user.verifyEmail.addPassword.success": "Password has been created",
  "user.registration.passwordTooWeak": "Password is too weak",
  "user.resetPassword.success": "Password has been reset, please check your email", 
  "user.resetPassword.failed": "Password reset failed",
  "user.resetPassword.enterEmail": "Please enter your email address",
  "user.resetPassword.emailAddress": "Email address",
  "user.resetPassword.submit": "Reset my password"
};

export const stringKeys = {
  login: {
    signIn: "login.signIn",
    password: "login.password",
    forgotPassword: "login.forgotPassword",
    notSucceeded: "login.notSucceeded",
    lockedOut: "login.lockedOut"
  },
  user: {
    verifyEmail: {
      setPassword: "user.verifyEmail.setPassword",
      welcome: "user.verifyEmail.welcome",
      signIn: "user.verifyEmail.signIn",
      password: "user.verifyEmail.password",
      failed: "user.verifyEmail.failed"
    },
    registration: {
      passwordTooWeak: "user.registration.passwordTooWeak"
    },
    resetPassword: {
      success: "user.resetPassword.success",
      failed: "user.resetPassword.failed",
      enterEmail: "user.resetPassword.enterEmail",
      emailAddress: "user.resetPassword.emailAddress",
      submit: "user.resetPassword.submit"
    }
  }
};

export const strings = (key) => {
  const value = stringList[key];
  return value === undefined ? key : value;
}

export function updateStrings(strings) {
  Object.assign(stringList, strings);
}

export default stringList;
