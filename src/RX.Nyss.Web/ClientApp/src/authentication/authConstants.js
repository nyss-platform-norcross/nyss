import { action } from "../utils/actions";

export const LOGIN = action("LOGIN");
export const LOGOUT = action("LOGOUT");
export const VERIFY_EMAIL = action("VERIFY_EMAIL");
export const RESET_PASSWORD = action("RESET_PASSWORD");
export const RESET_PASSWORD_CALLBACK = action("RESET_PASSWORD_CALLBACK");

export const localStorageUserIdKey = "userId";
