namespace RX.Nyss.Common.Configuration
{
    public interface IConfig : IBlobConfig
    {
        ConnectionStringOptions ConnectionStrings { get; set; }
        LoggingOptions Logging { get; set; }
        ServiceBusQueuesOptions ServiceBusQueues { get; set; }
    }
}
