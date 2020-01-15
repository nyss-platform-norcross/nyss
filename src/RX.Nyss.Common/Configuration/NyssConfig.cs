namespace RX.Nyss.Common.Configuration
{
    public interface INyssConfig : IConfig<NyssConfig.ConnectionStringOptions, NyssConfig.ServiceBusQueuesOptions>
    {
        string BaseUrl { get; set; }
        string Environment { get; set; }
        bool IsProduction { get; }
        string AuthorizedApiKeysBlobObjectName { get; set; }
        string EmailContentResourcesBlobObjectName { get; set; }
        int PaginationRowsPerPage { get; set; }
        NyssConfig.AuthenticationOptions Authentication { get; set; }
        NyssConfig.ExportOptions Export { get; set; }
        NyssConfig.ViewOptions View { get; set; }
    }

    public class NyssConfig : INyssConfig
    {
        public string BaseUrl { get; set; }

        public string Environment { get; set; }

        public bool IsProduction => Environment == NyssEnvironments.Prod;

        public string SmsGatewayBlobContainerName { get; set; }

        public string GeneralBlobContainerName { get; set; }

        public string AuthorizedApiKeysBlobObjectName { get; set; }

        public string StringsResourcesBlobObjectName { get; set; }

        public string EmailContentResourcesBlobObjectName { get; set; }

        public int PaginationRowsPerPage { get; set; }

        public string SmsContentResourcesBlobObjectName { get; set; }

        public ILoggingOptions Logging { get; set; }

        public NyssConfig.ConnectionStringOptions ConnectionStrings { get; set; }

        public AuthenticationOptions Authentication { get; set; }

        public ServiceBusQueuesOptions ServiceBusQueues { get; set; }

        public ExportOptions Export { get; set; }

        public ViewOptions View { get; set; }

        public class AuthenticationOptions
        {
            public string CookieName { get; set; }
            public int CookieExpirationTime { get; set; }
        }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
            public string ServiceBus { get; set; }
            public string GeneralBlobContainer { get; set; }
            public string SmsGatewayBlobContainer { get; set; }
            public string Nominatim { get; set; }
        }

        public class ServiceBusQueuesOptions : IServiceBusQueuesOptions
        {
            public string SendEmailQueue { get; set; }
            public string ReportDismissalQueue { get; set; }
        }

        public class ExportOptions
        {
            public string CsvFieldSeparator { get; set; }
        }

        public class ViewOptions
        {
            public int NumberOfGroupedVillagesInProjectDashboard { get; set; }
        }
    }
}
