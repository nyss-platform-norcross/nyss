namespace RX.Nyss.Web.Utils.DataContract
{
    public static class ResultKey
    {
        public static class User
        {
            public static class Common
            {
                public const string UserNotFound = "user.common.userNotFound";
            }

            public static class Registration
            {
                public const string Success = "user.registration.success";
                public const string UserAlreadyExists = "user.registration.userAlreadyExists";
                public const string UserAlreadyInRole = "user.registration.userAlreadyInRole";
                public const string UserNotFound = "user.registration.userNotFound";
                public const string PasswordTooWeak = "user.registration.passwordTooWeak";
                public const string NationalSocietyDoesNotExist = "user.registration.nationalSocietyDoesNotExist";
                public const string NoAssignableUserWithThisEmailFound = "user.registration.noAssignableUserWithThisEmailFound";
                public const string UserIsNotAssignedToThisNationalSociety = "user.registration.userIsNotAssignedToThisNationalSociety";
                public const string UserIsAlreadyInThisNationalSociety = "user.registration.userIsAlreadyInThisNationalSociety";
                public const string UnknownError = "user.registration.unknownError";
            }

            public static class Supervisor
            {
                public const string ProjectDoesNotExistOrNoAccess = "user.registration.projectDoesNotExistOrSupervisorDoesntHaveAccess";
            }

            public static class ResetPassword
            {
                public const string Success = "user.resetPassword.success";
                public const string Failed = "user.resetPassword.failed";
                public const string UserNotFound = "user.resetPassword.notFound";
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
            public const string BirthGroupStartYearMustBeMulipleOf10 = "validation.validationError.birthGroupStartYearMustBeMulipleOf10";
        }

        public static class NationalSociety
        {
            public const string NotFound = "nationalSociety.notFound";

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

            public static class SmsGateway
            {
                public const string SuccessfullyAdded = "nationalSociety.smsGateway.successfullyAdded";
                public const string SuccessfullyUpdated = "nationalSociety.smsGateway.successfullyUpdated";
                public const string SuccessfullyDeleted = "nationalSociety.smsGateway.successfullyDeleted";
                public const string ApiKeyAlreadyExists = "nationalSociety.smsGateway.apiKeyAlreadyExists";
                public const string SettingDoesNotExist = "nationalSociety.smsGateway.settingDoesNotExist";
                public const string NationalSocietyDoesNotExist = "nationalSociety.smsGateway.nationalSocietyDoesNotExist";
            }

            public static class SetHead
            {
                public const string NotAMemberOfSociety = "nationalSociety.setHead.notAMemberOfSociety";
                public const string NotApplicableUserRole = "nationalSociety.setHead.notApplicableUserRole";
            }
        }

        public static class HealthRisk
        {
            public const string CreationSuccess = "healthRisk.create.success";
            public const string CreationError = "healthRisk.create.error";
            public const string EditSuccess = "healthRisk.edit.success";
            public const string EditError = "healthRisk.edit.error";
            public const string RemoveSuccess = "healthRisk.remove.success";
            public const string HealthRiskNotFound = "healthRisk.notFound";
            public const string HealthRiskNumberAlreadyExists = "healthRisk.healthRiskNumberAlreadyExists";
        }

        public static class Project
        {
            public const string NotFound = "project.notFound";
            public const string SuccessfullyAdded = "project.successfullyAdded";
            public const string SuccessfullyUpdated = "project.successfullyUpdated";
            public const string SuccessfullyDeleted = "project.successfullyDeleted";
            public const string ProjectDoesNotExist = "project.projectDoesNotExist";
            public const string NationalSocietyDoesNotExist = "project.nationalSocietyDoesNotExist";
            public const string HealthRiskDoesNotExist = "project.healthRiskDoesNotExist";
            public const string HealthRiskContainsReports = "project.healthRiskContainsReports";
        }

        public const string UnexpectedError = "error.unexpected";
        
        public static class DataCollector
        {
            public const string CreateSuccess = "dataCollector.create.success";
            public const string EditSuccess = "dataCollector.edit.success";
            public const string PhoneNumberAlreadyExists = "dataCollector.phoneNumberExists";
            public const string ProjectDoesntExist = "dataCollector.projectDoesntExist";
            public const string DataCollectorNotFound = "dataCollector.notFound";
            public const string CreateError = "dataCollector.create.error";
            public const string EditError = "dataCollector.edit.error";
            public const string RemoveSuccess = "dataCollector.remove.success";
            public const string RemoveError = "dataCollector.remove.error";
        }

        public static class Geolocation
        {
            public const string NotFound = "There were no matching results when retrieving data from Nominatim API";
        }

    }
}
