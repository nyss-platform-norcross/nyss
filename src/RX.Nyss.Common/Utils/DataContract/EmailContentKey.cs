namespace RX.Nyss.Common.Utils.DataContract
{
    public static class EmailContentKey
    {
        public static class AlertHasNotBeenHandled
        {
            public const string Subject = "email.alertHasNotBeenHandled.subject";
            public const string Body = "email.alertHasNotBeenHandled.body";
        }

        public static class AlertEscalated
        {
            public const string Subject = "email.alertEscalated.subject";
            public const string Body = "email.alertEscalated.body";
        }

        public class ResetPassword
        {
            public const string Subject = "email.reset.subject";
            public const string Body = "email.reset.body";
        }

        public class EmailVerification
        {
            public const string Subject = "email.verification.subject";
            public const string Body = "email.verification.body";
            public const string DataConsumerBody = "email.dataConsumerVerification.body";
        }

        public static class Consent
        {
            public const string Subject = "email.consent.subject";
            public const string Body = "email.consent.body";
        }

        public static class ReplacedSupervisor
        {
            public const string Subject = "email.replacedSupervisor.subject";

            public const string Body = "email.replacedSupervisor.body";
        }
    }
}
