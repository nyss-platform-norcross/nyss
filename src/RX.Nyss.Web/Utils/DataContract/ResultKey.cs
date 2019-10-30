namespace RX.Nyss.Web.Utils.DataContract
{
    public static class ResultKey
    {
        public static class User
        {
            public static class Registration
            {
                public const string Success = "user.registration.success";
                public const string UserAlreadyExists = "user.registration.userAlreadyExists";
                public const string UserAlreadyInRole = "user.registration.userAlreadyInRole";
                public const string UserNotFound = "user.registration.userNotFound";
                public const string PasswordTooWeak = "user.registration.passwordTooWeak";
                public const string UnknownError = "user.registration.unknownError";
            }
        }

        public static class Login
        {
            public const string NotSucceeded = "login.notSucceeded";
            public const string LockedOut = "login.lockedOut";
        }

        public static class Validation
        {
            public const string ValidationError = "validation.validationError";
        }

        public static class NationalSociety
        {
            public static class Creation
            {
                public const string Success = "nationalSociety.creation.success";
                public const string NameNotDefined = "nationalSociety.creation.nameNotDefined";
                public const string CountryNotDefined = "nationalSociety.creation.countryNotDefined";
                public const string LanguageNotDefined = "nationalSociety.creation.languageNotDefined";
                public const string NameAlreadyExists = "nationalSociety.creation.nameAlreadyExists ";
                public const string Error = "nationalSociety.creation.error";
            }

            public static class Edit
            {
                public const string Success = "nationalSociety.edit.success";
                public const string Error = "nationalSociety.edit.error";
            }

            public static class Remove
            {
                public const string Success = "nationalSociety.remove.success";
                public const string Error = "nationalSociety.remove.error";
            }
        }
    }
}
