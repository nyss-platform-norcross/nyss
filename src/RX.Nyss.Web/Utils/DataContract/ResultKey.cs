namespace RX.Nyss.Web.Utils.DataContract
{
    public static class ResultKey
    {
        public static class Shared
        {
            public const string GeneralErrorMessage = "shared.error";
        }
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

            public static class ResetPassword
            {
                public const string Success = "user.resetPassword.success";
                public const string Failed = "user.resetPassword.failed";
            }

            public static class VerifyEmail
            {
                public const string Success = "user.verifyEmail.success";
                public const string Failed = "user.verifyEmail.failed";
                public const string NotFound = "user.verifyEmail.notFound";

                public static class AddPassword
                {
                    public const string Success = "user.verifyEmail.addPassword.success";
                    public const string Failed = "user.verifyEmail.addPassword.failed";
                }
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
                public const string CountryNotFound = "nationalSociety.creation.countryNotFound";
                public const string LanguageNotFound = "nationalSociety.creation.languageNotFound";
                public const string NameAlreadyExists = "nationalSociety.creation.nameAlreadyExists ";
            }

            public static class Edit
            {
                public const string Success = "nationalSociety.edit.success";
            }

            public static class Remove
            {
                public const string Success = "nationalSociety.remove.success";
            }
        }
    }
}
