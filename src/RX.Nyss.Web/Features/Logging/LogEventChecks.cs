using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class LogEventChecks
    {
        public static bool ContainsProperty(LogEvent le, string propertyName) => le.Properties.ContainsKey(propertyName);

        public static bool IsMicrosoft(LogEvent le) =>
            ContainsProperty(le, "SourceContext") &&
            le.Properties["SourceContext"].ToString().Contains("Microsoft.");
    }
}
