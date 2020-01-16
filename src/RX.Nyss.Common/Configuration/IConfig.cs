namespace RX.Nyss.Common.Configuration
{
    public interface IConfig : IBlobConfig
    {
        public ConnectionStringOptions ConnectionStrings { get; set; }
        public LoggingOptions Logging { get; set; }
        public ServiceBusQueuesOptions ServiceBusQueues { get; set; }
    }
}
