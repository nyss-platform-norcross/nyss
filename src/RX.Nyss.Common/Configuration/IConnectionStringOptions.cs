namespace RX.Nyss.Common.Configuration
{
    public interface IConnectionStringOptions : IDatabaseConnectionStringOptions, IBlobConnectionStringOptions, IServiceConnectionStringOptions, IExternalServicesConnectionStringOptions
    {
    }

    public interface IDatabaseConnectionStringOptions
    {
        string NyssDatabase { get; set; }
    }

    public interface IBlobConnectionStringOptions
    {
        string GeneralBlobContainer { get; set; }
        string SmsGatewayBlobContainer { get; set; }
        string DataBlobContainer { get; set; }
    }

    public interface IServiceConnectionStringOptions
    {
        string ServiceBus { get; set; }
    }

    public interface IExternalServicesConnectionStringOptions
    {
        string Nominatim { get; set; }
    }

    public class ConnectionStringOptions : IConnectionStringOptions
    {
        public string NyssDatabase { get; set; }
        public string GeneralBlobContainer { get; set; }
        public string DataBlobContainer { get; set; }
        public string SmsGatewayBlobContainer { get; set; }
        public string ServiceBus { get; set; }
        public string Nominatim { get; set; }
    }
}
