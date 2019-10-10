namespace RX.Nyss.Web
{
    public class NyssConfig
    {
        public LoggingOptions Logging { get; set; }
        public ConnectionStringOptions ConnectionStrings { get; set; }

        public interface ILoggingOptions
        {
            string LogsLocation { get; set; }
        }
        public interface IConnectionStringOptions
        {
             string NyssDatabase { get; set; }
        }

        public class LoggingOptions : ILoggingOptions
        {
            public string LogsLocation { get; set; }
        }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
        }
    }
}
