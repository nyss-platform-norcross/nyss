namespace RX.Nyss.Web.Configuration
{
    public class NyssConfig
    {
        public LoggingOptions Logging { get; set; }

        public ConnectionStringOptions ConnectionStrings { get; set; }

        public AuthenticationOptions Authentication { get; set; }

        public interface ILoggingOptions
        {
            string LogsLocation { get; set; }
            string LogMessageTemplate { get; set; }
        }

        public interface IAuthenticationOptions
        {
            string Secret { get; set; }
            string Issuer { get; set; }
            string Audience { get; set; }
        }

        public interface IConnectionStringOptions
        {
             string NyssDatabase { get; set; }
        }

        public class LoggingOptions : ILoggingOptions
        {
            public string LogsLocation { get; set; }
            public string LogMessageTemplate { get; set; }
        }

        public class AuthenticationOptions : IAuthenticationOptions
        {
            public string Secret { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
        }
    }
}
