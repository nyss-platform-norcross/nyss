namespace RX.Nyss.Common.Configuration
{
    public interface IConfig<TConnectionStringOptions>
        where TConnectionStringOptions : IConnectionStringOptions
        //where TServiceBusQueuesOptions : IServiceBusQueuesOptions
    {
        string GeneralBlobContainerName { get; set; }
        string StringsResourcesBlobObjectName { get; set; }
        string SmsContentResourcesBlobObjectName { get; set; }
        TConnectionStringOptions ConnectionStrings { get; set; }
        //TServiceBusQueuesOptions ServiceBusQueues { get; set; }
    }
}
