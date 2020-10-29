using RX.Nyss.Common.Configuration;

namespace RX.Nyss.Web.Configuration
{
    public interface INyssWebConfig : IConfig
    {
        string BaseUrl { get; set; }
        string FuncAppBaseUrl { get; set; }
        string Environment { get; set; }
        bool IsProduction { get; }
        bool IsDemo { get; }
        string AuthorizedApiKeysBlobObjectName { get; set; }
        int PaginationRowsPerPage { get; set; }
        ConfigSingleton.AuthenticationOptions Authentication { get; set; }
        ConfigSingleton.ExportOptions Export { get; set; }
        ConfigSingleton.ViewOptions View { get; set; }
    }

    public class ConfigSingleton : INyssWebConfig
    {
        public string BaseUrl { get; set; }

        public string FuncAppBaseUrl { get; set; }

        public string Languages { get; set; }

        public string Environment { get; set; }

        public bool IsProduction => Environment == NyssEnvironments.Prod || Environment == NyssEnvironments.Demo;

        public bool IsDemo => Environment == NyssEnvironments.Demo;

        public string SmsGatewayBlobContainerName { get; set; }

        public string PlatformAgreementsContainerName { get; set; }

        public string PublicStatsBlobContainerName { get; set; }

        public string GeneralBlobContainerName { get; set; }

        public string AuthorizedApiKeysBlobObjectName { get; set; }

        public string StringsResourcesBlobObjectName { get; set; }

        public string EmailContentResourcesBlobObjectName { get; set; }

        public string PlatformAgreementBlobObjectName { get; set; }

        public string PublicStatsBlobObjectName { get; set; }

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
