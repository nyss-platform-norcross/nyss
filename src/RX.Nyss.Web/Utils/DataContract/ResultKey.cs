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

            public static class Seeding
            {
                public const string RoleCouldNotBeCreated = "user.seeding.roleCouldNotBeCreated";
            }
        }

        public static class Validation
        {
            public const string ValidationError = "validation.validationError";
        }
    }
}
