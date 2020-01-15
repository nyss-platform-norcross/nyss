namespace RX.Nyss.Common.Configuration
{
    public interface IConnectionStringOptions : ICommonConnectionStringOptions
    {

    }
    public interface ICommonConnectionStringOptions
    {
        string NyssDatabase { get; set; }
        string ServiceBus { get; set; }
    }
}
