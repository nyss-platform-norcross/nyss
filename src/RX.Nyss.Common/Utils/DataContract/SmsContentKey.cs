namespace RX.Nyss.Common.Utils.DataContract
{
    public static class SmsContentKey
    {
        public static class ReportError
        {
            public const string FormatError = "sms.error.formatError";
            public const string HealthRiskNotFound = "sms.error.healthRiskNotFound";
            public const string Other = "sms.error.other";
        }

        public static class Alerts
        {
            public const string AlertEscalated = "sms.alertEscalated";
        }
    }
}
