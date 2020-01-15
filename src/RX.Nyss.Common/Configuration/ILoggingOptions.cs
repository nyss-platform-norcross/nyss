namespace RX.Nyss.Common.Configuration
{
    public interface ILoggingConfig<TLoggingOptions> where TLoggingOptions : ILoggingOptions
    {
        TLoggingOptions Logging { get; set; }
    }
    public interface ILoggingOptions
    {
        string LogsLocation { get; set; }
        string LogMessageTemplate { get; set; }
        string LogFile { get; set; }
    }
}
