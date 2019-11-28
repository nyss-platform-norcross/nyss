namespace RX.Nyss.ReportApi.Configuration
{
    public interface IConfig
    {
        NyssReportApiConfig.LoggingOptions Logging { get; set; }
        NyssReportApiConfig.ConnectionStringOptions ConnectionStrings { get; set; }
        NyssReportApiConfig.ServiceBusQueuesOptions ServiceBusQueues { get; set; }
        string GeneralBlobContainerName { get; set; }
        string StringsResourcesBlobObjectName { get; set; }
    }

    public class NyssReportApiConfig : IConfig
    {
        public LoggingOptions Logging { get; set; }

        public ConnectionStringOptions ConnectionStrings { get; set; }

        public ServiceBusQueuesOptions ServiceBusQueues { get; set; }

        public string GeneralBlobContainerName { get; set; }

        public string StringsResourcesBlobObjectName { get; set; }

        public class LoggingOptions 
        {
            public string LogsLocation { get; set; }
            public string LogMessageTemplate { get; set; }
        }

        public class ConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
            public string ServiceBus { get; set; }
            public string GeneralBlobContainer { get; set; }
        }

        public class ServiceBusQueuesOptions
        {
            public string SendEmailQueue { get; set; }
        }
    }
}
