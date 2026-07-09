namespace RX.Nyss.Common.Configuration;

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
    string IotHubManagement { get; set; }
    string IotHubService { get; set; }
}

public interface IExternalServicesConnectionStringOptions
{
    string Nominatim { get; set; }
}

public class ConnectionStringOptions : IConnectionStringOptions
{
    public string NyssDatabase { get; set; } = string.Empty;
    public string GeneralBlobContainer { get; set; } = string.Empty;
    public string DataBlobContainer { get; set; } = string.Empty;
    public string SmsGatewayBlobContainer { get; set; } = string.Empty;
    public string ServiceBus { get; set; } = string.Empty;
    public string Nominatim { get; set; } = string.Empty;
    public string IotHubManagement { get; set; } = string.Empty;
    public string IotHubService { get; set; } = string.Empty;
}