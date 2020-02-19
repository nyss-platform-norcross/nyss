using RX.Nyss.Common.Configuration;

namespace RX.Nyss.Web.Configuration
{
    public interface INyssWebConfig : IConfig
    {
        string BaseUrl { get; set; }
        string Environment { get; set; }
        bool IsProduction { get; }
        string AuthorizedApiKeysBlobObjectName { get; set; }
        string FeedbackReceiverEmail { get; set; }
        int PaginationRowsPerPage { get; set; }
        ConfigSingleton.AuthenticationOptions Authentication { get; set; }
        ConfigSingleton.ExportOptions Export { get; set; }
        ConfigSingleton.ViewOptions View { get; set; }
    }

    public class ConfigSingleton : INyssWebConfig
    {
        public string BaseUrl { get; set; }

        public string Environment { get; set; }

        public bool IsProduction => Environment == NyssEnvironments.Prod;

        public string SmsGatewayBlobContainerName { get; set; }

        public string GeneralBlobContainerName { get; set; }

        public string AuthorizedApiKeysBlobObjectName { get; set; }

        public string FeedbackReceiverEmail { get; set; }

        public string StringsResourcesBlobObjectName { get; set; }

        public string EmailContentResourcesBlobObjectName { get; set; }

        public string PlatformAgreementBlobObjectName { get; set; }

        public int PaginationRowsPerPage { get; set; }

        public string SmsContentResourcesBlobObjectName { get; set; }

        public LoggingOptions Logging { get; set; }

        public ConnectionStringOptions ConnectionStrings { get; set; }

        public ServiceBusQueuesOptions ServiceBusQueues { get; set; }

        public AuthenticationOptions Authentication { get; set; }

        public ExportOptions Export { get; set; }

        public ViewOptions View { get; set; }

        public class AuthenticationOptions
        {
            public string CookieName { get; set; }
            public int CookieExpirationTime { get; set; }
        }

        public class ExportOptions
        {
            public string CsvFieldSeparator { get; set; }
        }

        public class ViewOptions
        {
            public int NumberOfGroupedVillagesInProjectDashboard { get; set; }
            public int NumberOfGroupedHealthRisksInDashboard { get; set; }
        }
    }
}
