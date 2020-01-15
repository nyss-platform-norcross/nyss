namespace RX.Nyss.Common.Configuration
{
    public interface IBlobConfig : IConfig<BlobConfig.ConnectionStringOptions>
    {
        // string GeneralBlobContainerName { get; set; }
        // string SmsContentResourcesBlobObjectName { get; set; }
        // string StringsResourcesBlobObjectName { get; set; }
        // BlobConfig.ConnectionStringOptions ConnectionStrings { get; set; }
    }

    public class BlobConfig : IBlobConfig
    {
        public string GeneralBlobContainerName { get; set; }
        public string SmsContentResourcesBlobObjectName { get; set; }
        public string StringsResourcesBlobObjectName { get; set; }
        public ConnectionStringOptions ConnectionStrings { get; set; }

        public class ConnectionStringOptions : IConnectionStringOptions
        {
            public string NyssDatabase { get; set; }
            public string ServiceBus { get; set; }
            public string GeneralBlobContainer { get; set; }
        }
    }
}
