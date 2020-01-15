namespace RX.Nyss.Common.Configuration
{
    public interface ILoggingOptions
    {
        string LogsLocation { get; set; }
        string LogMessageTemplate { get; set; }
        string LogFile { get; set; }
    }

    public class LoggingOptions : ILoggingOptions
    {
        public string LogsLocation { get; set; }
        public string LogMessageTemplate { get; set; }
        public string LogFile { get; set; }
    }
}
