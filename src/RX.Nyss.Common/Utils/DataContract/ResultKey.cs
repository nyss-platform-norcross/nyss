using System.Data;

namespace RX.Nyss.Common.Utils.DataContract
{
    public static class ResultKey
    {
        public const string UnexpectedError = "error.unexpected";
        public const string Unauthorized = "error.unauthorized";

        public static class User
        {
            public static class Common
            {
                public const string UserNotFound = "user.common.userNotFound";
                public const string OnlyCoordinatorCanChangeTheOrganizationOfAnotherUser = "user.common.onlyCoordinatorCanChangeTheOrganizationOfAnotherUser";
                public const string CoordinatorCanChangeTheOrganizationOnlyForHeadManager = "user.common.coordinatorCanChangeTheOrganizationOnlyForHeadManager";
            }

            public static class Deletion
            {
                public const string NoPermissionsToDeleteThisUser = "user.deletion.noPermissionsToDeleteThisUser";
                public const string CannotDeleteHeadManager = "user.deletion.cannotDeleteHeadManager";
                public const string CannotDeleteHeadManagerWithUsers = "user.deletion.cannotDeleteHeadManagerWithUsers";
                public const string CannotDeleteSupervisorWithDataCollectors = "user.deletion.cannotDeleteSupervisorWithDataCollectors";
                public const string CannotDeleteHeadSupervisorHasSupervisors = "user.deletion.cannotDeleteHeadSupervisorHasSupervisors";
                public const string CannotDeleteYourself = "user.deletion.cannotDeleteYourself";
                public const string MoreUsersExists = "user.deletion.moreUsersExists";
                public const string UserNotFound = "user.deletion.userNotFound ";
            }

            public static class Registration
            {
                public const string Success = "user.registration.success";
                public const string UserAlreadyExists = "user.registration.userAlreadyExists";
                public const string UserAlreadyInRole = "user.registration.userAlreadyInRole";
                public const string UserNotFound = "user.registration.userNotFound";
                public const string PasswordTooWeak = "user.registration.passwordTooWeak";
                public const string NationalSocietyDoesNotExist = "user.registration.nationalSocietyDoesNotExist";
                public const string CannotCreateUsersInArchivedNationalSociety = "user.registration.cannotCreateUsersInArchivedNationalSociety";
                public const string CannotAddExistingUsersToArchivedNationalSociety = "user.registration.cannotAddExistingUsersToArchivedNationalSociety";
                public const string NoAssignableUserWithThisEmailFound = "user.registration.noAssignableUserWithThisEmailFound";
                public const string TechnicalAdvisorsCanBeAttachedOnlyByManagers = "user.registration.technicalAdvisorsCanBeAttachedOnlyByManagers";
                public const string UserIsNotAssignedToThisNationalSociety = "user.registration.userIsNotAssignedToThisNationalSociety";
                public const string UserIsAlreadyInThisNationalSociety = "user.registration.userIsAlreadyInThisNationalSociety";
                public const string UnknownError = "user.registration.unknownError";
                public const string NationalSocietyCoordinatorAlreadyExists = "user.registration.nationalSocietyCoordinatorAlreadyExists";
                public const string NotHeadManager = "user.registration.notHeadManager";
                public const string HeadManagerAlreadyExists = "user.registration.headManagerAlreadyExists";
                public const string CoordinatorExists = "user.registration.coordinatorExists";
                public const string CannotAssignUserToModemInDifferentNationalSociety = "user.registration.cannotAssignUserToModemInDifferentNationalSociety";
            }

            public static class Supervisor
            {
                public const string ProjectDoesNotExistOrNoAccess = "user.registration.projectDoesNotExistOrSupervisorDoesntHaveAccess";
                public const string CannotChangeProjectSupervisorHasDataCollectors = "user.registration.cannotChangeProjectSupervisorHasDataCollectors";
            }

            public static class HeadSupervisor
            {
                public const string CannotChangeProjectHeadSupervisorHasSupervisors = "user.registration.cannotChangeProjectHeadSupervisorHasSupervisors";
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

        public static class Organization
        {
            public const string NoAccessToChangeOrganization = "organization.notAccessToChangeOrganization";
            public const string NameAlreadyExists = "organization.nameAlreadyExists";
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
                public const string NameAlreadyExists = "nationalSociety.creation.nameAlreadyExists";
            }

            public static class Edit
            {
                public const string Success = "nationalSociety.edit.success";
                public const string CannotEditArchivedNationalSociety = "nationalSociety.edit.cannotEditArchivedNationalSociety";
            }

            public static class Remove
            {
                public const string Success = "nationalSociety.remove.success";
            }

            public static class Archive
            {
                public const string Success = "nationalSociety.archive.success";
                public const string ErrorHasOpenedProjects = "nationalSociety.archive.errorHasOpenedProjects";
                public const string ErrorHasRegisteredUsers = "nationalSociety.archive.errorHasRegisteredUsers";
                public const string ReopenSuccess = "nationalSociety.archive.repoenSuccess";
            }

            public static class SmsGateway
            {
                public const string SuccessfullyAdded = "nationalSociety.smsGateway.successfullyAdded";
                public const string SuccessfullyUpdated = "nationalSociety.smsGateway.successfullyUpdated";
                public const string SuccessfullyDeleted = "nationalSociety.smsGateway.successfullyDeleted";
                public const string ApiKeyAlreadyExists = "nationalSociety.smsGateway.apiKeyAlreadyExists";
                public const string SettingDoesNotExist = "nationalSociety.smsGateway.settingDoesNotExist";
                public const string NationalSocietyDoesNotExist = "nationalSociety.smsGateway.nationalSocietyDoesNotExist";
                public const string IoTHubPingFailed = "nationalSociety.smsGateway.ioTHubPingFailed";
            }

            public static class Organization
            {
                public const string SuccessfullyAdded = "nationalSociety.smsGateway.successfullyAdded";
                public const string SuccessfullyUpdated = "nationalSociety.smsGateway.successfullyUpdated";
                public const string SuccessfullyDeleted = "nationalSociety.smsGateway.successfullyDeleted";
                public const string SettingDoesNotExist = "nationalSociety.smsGateway.settingDoesNotExist";
                public const string NationalSocietyDoesNotExist = "nationalSociety.smsGateway.nationalSocietyDoesNotExist";

                public static class Deletion
                {
                    public const string HasUsers = "nationalSociety.organization.deletion.hasUsers";
                    public const string LastOrganization = "nationalSociety.organization.deletion.lastOrganization ";
                }
            }

            public static class SetHead
            {
                public const string NotAMemberOfSociety = "nationalSociety.setHead.notAMemberOfSociety";
                public const string NotApplicableUserRole = "nationalSociety.setHead.notApplicableUserRole";
            }

            public static class Structure
            {
                public const string ItemAlreadyExists = "nationalSociety.structure.itemAlreadyExists";
                public const string CannotCreateItemInArchivedNationalSociety = "nationalSociety.structure.cannotCreateItemInArchivedNationalSociety";
            }
        }

        public static class HealthRisk
        {
            public const string HealthRiskNotFound = "healthRisk.notFound";
            public const string HealthRiskNumberAlreadyExists = "healthRisk.healthRiskNumberAlreadyExists";
            public const string HealthRiskContainsReports = "healthRisk.healthRiskContainsReports";

            public static class Create
            {
                public const string CreationSuccess = "healthRisk.create.success";
                public const string CreationError = "healthRisk.create.error";
            }

            public static class Edit
            {
                public const string EditSuccess = "healthRisk.edit.success";
                public const string EditError = "healthRisk.edit.error";
            }

            public static class Remove
            {
                public const string RemoveSuccess = "healthRisk.remove.success";
            }
        }

        public static class Report
        {
            public const string ProjectIsClosed = "report.projectIsClosed";
            public const string ReportNotFound = "report.reportNotFound";
            public const string NoAccess = "report.noAccess";
            public const string ReportSentSuccessfully = "report.sentSuccessfully";
            public const string NoGatewaySettingFoundForNationalSociety = "report.noGatewaySettingFoundForNationalSociety";
            public const string LinkedToSupervisor = "report.linkedToSupervisor";
            public const string LinkedToOrganization = "report.linkedToOrganization";
            public const string AlreadyCrossChecked = "report.alreadyCrossChecked";
            public const string CannotCrossCheckErrorReport = "report.cannotCrossCheckErrorReport";

            public static class Status
            {
                public const string New = "report.status.new";
                public const string Pending = "report.status.pending";
                public const string Rejected = "report.status.rejected";
                public const string Accepted = "report.status.accepted";
                public const string Closed = "report.status.closed";
            }

            public static class Edit
            {
                public const string EditSuccess = "report.edit.success";
                public const string EditError = "report.edit.error";
                public const string SenderEditError = "report.edit.senderEditError";
                public const string SenderDoesNotExist = "report.edit.senderDoesNotExist";
                public const string OnlyNewReportsEditable = "report.edit.onlyNewReportsEditable";
                public const string HealthRiskNotAssignedToProject = "report.healthRiskNotAssignedToProject";
                public const string HealthRiskCannotBeEdited = "report.healthRiskCannotBeEdited";
            }

            public static class ErrorType
            {
                public const string HealthRiskNotFound = "report.errorType.healthRiskNotFound";
                public const string GlobalHealthRiskCodeNotFound = "report.errorType.globalHealthRiskCodeNotFound";
                public const string FormatError = "report.errorType.formatError";
                public const string Gateway = "report.errorType.gateway";
                public const string TooLong = "report.errorType.tooLong";
                public const string DataCollectorUsedCollectionPointFormat = "report.errorType.dataCollectorUsedCollectionPointFormat";
                public const string CollectionPointUsedDataCollectorFormat = "report.errorType.collectionPointUsedDataCollectorFormat";
                public const string CollectionPointNonHumanHealthRisk = "report.errorType.collectionPointNonHumanHealthRisk";
                public const string SingleReportNonHumanHealthRisk = "report.errorType.singleReportNonHumanHealthRisk";
                public const string AggregateReportNonHumanHealthRisk = "report.errorType.aggregateReportNonHumanHealthRisk";
                public const string EventReportHumanHealthRisk = "report.errorType.eventReportHumanHealthRisk";
                public const string GenderAndAgeNonHumanHealthRisk = "report.errorType.genderAndAgeNonHumanHealthRisk";
                public const string Other = "report.errorType.other";
            }
        }

        public static class Project
        {
            public const string NotFound = "project.notFound";
            public const string SuccessfullyAdded = "project.successfullyAdded";
            public const string SuccessfullyUpdated = "project.successfullyUpdated";
            public const string SuccessfullyClosed = "project.successfullyClosed";
            public const string ProjectDoesNotExist = "project.projectDoesNotExist";
            public const string ProjectAlreadyClosed = "project.projectAlreadyClosed";
            public const string NationalSocietyDoesNotExist = "project.nationalSocietyDoesNotExist";
            public const string HealthRiskDoesNotExist = "project.healthRiskDoesNotExist";
            public const string HealthRiskContainsReports = "project.healthRiskContainsReports";
            public const string CannotAddProjectInArchivedNationalSociety = "project.cannotAddProjectInArchivedNationalSociety";
            public const string ProjectHasOpenOrEscalatedAlerts = "project.projectHasOpenOrEscalatedAlerts";
            public const string CannotRemoveAlertRecipientWithSupervisorsAttached = "project.cannotRemoveAlertRecipientWithSupervisorsAttached";
            public const string OnlyCoordinatorCanAdministrateProjects = "project.onlyCoordinatorCanAdministrateProjects";
            public const string OnlyCoordinatorCanCloseThisProjects = "project.onlyCoordinatorCanCloseThisProjects";
            public const string AllowMultipleOrganizationsFlagCannotBeRemoved = "project.allowMultipleOrganizationsFlagCannotBeRemoved";
            public const string OnlyCoordinatorCanChangeMultipleOrgAccess = "project.onlyCoordinatorCanChangeMultipleOrgAccess";
            public const string NoAccessToSetOrgBasedAccessControl = "project.noAccessToSetOrgBasedAccessControl";
            public const string AlertNotHandledNotificationRecipientMustBeOfSameOrg = "project.alertNotHandledNotificationRecipientMustBeOfSameOrg";
            public const string AlertNotHandledRecipientDoesNotExist = "project.alertNotHandledRecipientDoesNotExist";
        }

        public class ProjectOrganization
        {
            public const string ProjectDoesNotAllowAddingMultipleOrganizations = "projectOrganization.projectDoesNotAllowAddingMultipleOrganizations";
            public const string OrganizationAlreadyAdded = "projectOrganization.organizationAlreadyAdded";
            public const string CannotRemoveLastOrganization = "projectOrganization.cannotRemoveLastOrganization";
            public const string CannotRemoveOrganizationThatHasSupervisors = "projectOrganization.cannotRemoveOrganizationThatHasSupervisors";
            public const string ProjectIsClosed = "projectOrganization.projectIsClosed";
        }

        public static class AlertRecipient
        {
            public const string AlertRecipientAlreadyAdded = "alertRecipient.alertRecipientAlreadyAdded";
            public const string AlertRecipientDoesNotExist = "alertRecipient.alertRecipientDoesNotExist";
            public const string AlertRecipientSuccessfullyEdited = "alertRecipient.alertRecipientSuccessfullyEdited";
            public const string CurrentUserMustBeTiedToAnOrganization = "alertRecipient.currentUserMustBeTiedToAnOrganization";
            public const string CannotDeleteAlertRecipientTiedToSupervisors = "alertRecipient.cannotDeleteAlertRecipientTiedToSupervisors";
            public const string ProjectIsClosed = "alertRecipient.projectIsClosed";
            public const string AllSupervisorsMustBeTiedToSameOrganization = "alertRecipient.allSupervisorsMustBeTiedToSameOrganization ";
            public const string AllHeadSupervisorsMustBeTiedToSameOrganization = "alertRecipient.allHeadSupervisorsMustBeTiedToSameOrganization ";
            public const string ModemMustBeConnectedToSameNationalSociety = "alertRecipient.modemMustBeConnectedToSameNationalSociety";
        }

        public class SqlExceptions
        {
            public const string GeneralError = "error.sql.general";
            public const string ForeignKeyViolation = "error.sql.foreignKeyViolation";
            public const string DuplicatedValue = "error.sql.duplicatedValue";
        }

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
            public const string SetInTrainingSuccess = "dataCollector.setInTraining.success";
            public const string SetOutOfTrainingSuccess = "dataCollector.setOutOfTraining.success";
            public const string ProjectIsClosed = "dataCollector.projectIsClosed";
            public const string NotAllowedToSelectSupervisor = "dataCollector.notAllowedToSelectSupervisor";
            public const string SetToDeployedSuccess = "dataCollector.setToDeployed.success";
            public const string SetToNotDeployedSuccess = "dataCollector.setToNotDeployed.success";
        }

        public static class Geolocation
        {
            public const string NotFound = "There were no matching results when retrieving data from Nominatim API";
        }

        public static class Alert
        {
            public const string InconsistentReportData = "alert.inconsistentReportData";

            public static class AcceptReport
            {
                public const string WrongAlertStatus = "alert.acceptReport.wrongAlertStatus";
                public const string WrongReportStatus = "alert.acceptReport.wrongReportStatus";
                public const string NoPermission = "alert.acceptReport.noPermission";
            }

            public static class DismissReport
            {
                public const string WrongAlertStatus = "alert.dismissReport.wrongAlertStatus";
                public const string WrongReportStatus = "alert.dismissReport.wrongReportStatus";
                public const string NoPermission = "alert.dismissReport.noPermission";
            }

            public static class ResetReport
            {
                public const string WrongAlertStatus = "alert.resetReport.wrongAlertStatus";
                public const string WrongReportStatus = "alert.resetReport.wrongReportStatus";
                public const string NoPermission = "alert.resetReport.noPermission";
                public const string ReportWasCrossCheckedBeforeAlertEscalation = "alert.resetReport.reportWasCrossCheckedBeforeAlertEscalation";
            }

            public static class EscalateAlert
            {
                public const string ThresholdNotReached = "alert.escalateAlert.thresholdNotReached";
                public const string WrongStatus = "alert.escalateAlert.wrongStatus";
                public const string EmailNotificationFailed = "alert.escalateAlert.emailNotifcationFailed";
                public const string SmsNotificationFailed = "alert.escalateAlert.smsNotifcationFailed";
                public const string Success = "alert.escalateAlert.success";
                public const string NoPermission = "alert.escalateAlert.noPermission";
            }

            public static class DismissAlert
            {
                public const string WrongStatus = "alert.dismissAlert.wrongStatus";
                public const string PossibleEscalation = "alert.dismissAlert.possibleEscalation";
                public const string NoPermission = "alert.dismissAlert.noPermission";
            }

            public static class CloseAlert
            {
                public const string WrongStatus = "alert.closeAlert.wrongStatus";
                public const string NoPermission = "alert.closeAlert.noPermission";
            }
        }

        public static class Consent
        {
            public const string NoPendingConsent = "consent.noPendingConsent";
        }

        public static class AlertNotHandledNotificationRecipient
        {
            public const string CreateSuccess = "alertNotHandledRecipient.create.success";
            public const string EditSuccess = "alertNotHandledRecipient.edit.success";
            public const string DeleteSuccess = "alertNotHandledRecipient.delete.success";
            public const string UserMustBeInSameOrg = "alertNotHandledRecipient.userMustBeInSameOrg";
            public const string NotFound = "alertNotHandledRecipient.notFound";
            public const string AlreadyExists = "alertNotHandledRecipient.alreadyExists";
        }
    }
}
