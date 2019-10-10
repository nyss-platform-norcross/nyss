using System.Text.RegularExpressions;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class LogEventChecks
    {
        public static bool ContainsProperty(LogEvent le, string propertyName)
        {
            return le.Properties.ContainsKey(propertyName);
        }

        public static bool IsRepositoryCall(LogEvent le)
        {
            return ContainsProperty(le, CallerLogEventEnricher.CallerPropertyName) && Regex.IsMatch(le.Properties[CallerLogEventEnricher.CallerPropertyName].ToString(), @"[a-zA-Z].*Repository\.[a-zA-Z].*");
        }

        public static bool IsHangfire(LogEvent le)
        {
            return ContainsProperty(le, "SourceContext") &&
                   le.Properties["SourceContext"].ToString().Contains("Hangfire.");
        }

        public static bool IsMicrosoft(LogEvent le)
        {
            return ContainsProperty(le, "SourceContext") &&
                   le.Properties["SourceContext"].ToString().Contains("Microsoft.");
        }

        public static bool IsMassTransit(LogEvent le)
        {
            return ContainsProperty(le, "SourceContext") &&
                   le.Properties["SourceContext"].ToString().Contains("MassTransit.");
        }
    }
}
