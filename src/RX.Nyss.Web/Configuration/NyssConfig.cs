namespace RX.Nyss.Web.Configuration
{
    public interface INyssConfig
    {
        NyssConfig.LoggingOptions Logging { get; set; }
        NyssConfig.ConnectionStringOptions ConnectionStrings { get; set; }
        NyssConfig.AuthenticationOptions Authentication { get; set; }
    }

    public class NyssConfig : INyssConfig
    {
        public LoggingOptions Logging { get; set; }

        public ConnectionStringOptions ConnectionStrings { get; set; }

        public AuthenticationOptions Authentication { get; set; }

        public class LoggingOptions 
        {
            public string LogsLocation { get; set; }
            public string LogMessageTemplate { get; set; }
        }

        public class AuthenticationOptions { 

            public string Secret { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }

        public class ConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
        }
    }
}
