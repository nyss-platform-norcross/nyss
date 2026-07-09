namespace RX.Nyss.Common.Configuration;

public interface ILoggingOptions
{
    string LogsLocation { get; set; }
    string LogMessageTemplate { get; set; }
    string LogFile { get; set; }
}

public class LoggingOptions : ILoggingOptions
{
    public string LogsLocation { get; set; } = "Logs";
    public string LogMessageTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    public string LogFile { get; set; } = "log.txt";
}