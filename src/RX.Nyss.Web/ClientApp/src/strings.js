let stringList = {
    "login.signIn": "Log innnn",
    "login.forgotPassword": "Forgot password?"
};

export function updateStrings(strings) {
    Object.assign(stringList, strings);
}

export default stringList;
