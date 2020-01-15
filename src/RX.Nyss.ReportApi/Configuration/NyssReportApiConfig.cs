using RX.Nyss.Common.Configuration;

namespace RX.Nyss.ReportApi.Configuration
{
    public interface INyssReportApiConfig : IConfig<NyssReportApiConfig.ConnectionStringOptions>, ILoggingConfig<NyssReportApiConfig.LoggingOptions>
    {
        string BaseUrl { get; set; }
        NyssReportApiConfig.ServiceBusQueuesOptions ServiceBusQueues { get; set; }
    }

    public class NyssReportApiConfig : INyssReportApiConfig
    {
        public string BaseUrl { get; set; }

        public NyssReportApiConfig.LoggingOptions Logging { get; set; }

        public NyssReportApiConfig.ConnectionStringOptions ConnectionStrings { get; set; }

        public NyssReportApiConfig.ServiceBusQueuesOptions ServiceBusQueues { get; set; }

        public string GeneralBlobContainerName { get; set; }

        public string StringsResourcesBlobObjectName { get; set; }

        public string SmsContentResourcesBlobObjectName { get; set; }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
            public string ServiceBus { get; set; }
            public string GeneralBlobContainer { get; set; }
        }

        public class LoggingOptions : ILoggingOptions
        {
            public string LogsLocation { get; set; }
            public string LogMessageTemplate { get; set; }
            public string LogFile { get; set; }
        }

        public class ServiceBusQueuesOptions : IServiceBusQueuesOptions
        {
            public string SendEmailQueue { get; set; }
        }
    }
}
