using RX.Nyss.Common.Configuration;

namespace RX.Nyss.ReportApi.Configuration
{
    public interface INyssReportApiConfig : IConfig
    {
        string BaseUrl { get; set; }

        int CheckAlertTimeoutInMinutes { get; set; }
    }

    public class ConfigSingleton : INyssReportApiConfig
    {
        public string BaseUrl { get; set; }
        public int CheckAlertTimeoutInMinutes { get; set; }
        public LoggingOptions Logging { get; set; }
        public ConnectionStringOptions ConnectionStrings { get; set; }
        public ServiceBusQueuesOptions ServiceBusQueues { get; set; }
        public string GeneralBlobContainerName { get; set; }
        public string SmsGatewayBlobContainerName { get; set; }
        public string PlatformAgreementsContainerName { get; set; }
        public string StringsResourcesBlobObjectName { get; set; }
        public string EmailContentResourcesBlobObjectName { get; set; }
        public string PlatformAgreementBlobObjectName { get; set; }
        public string SmsContentResourcesBlobObjectName { get; set; }
    }
}
