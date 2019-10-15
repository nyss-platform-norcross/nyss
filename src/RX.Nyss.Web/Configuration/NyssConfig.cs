namespace RX.Nyss.Web.Configuration
{
    public class NyssConfig
    {
        public LoggingOptions Logging { get; set; }

        public ConnectionStringOptions ConnectionStrings { get; set; }

        public interface ILoggingOptions
        {
            string LogsLocation { get; set; }
            string LogMessageTemplate { get; set; }
            string ApplicationInsightsInstrumentationKey { get; set; }
        }

        public interface IConnectionStringOptions
        {
             string NyssDatabase { get; set; }
        }

        public class LoggingOptions : ILoggingOptions
        {
            public string LogsLocation { get; set; }
            public string LogMessageTemplate { get; set; }
            public string ApplicationInsightsInstrumentationKey { get; set; }
        }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
        }
    }
}
