using System.IO;
using RX.Nyss.Common.Configuration;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Common.Utils.Logging;

public static class GlobalLoggerConfiguration
{
    public static void ConfigureLogger(ILoggingOptions loggingOptions)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: loggingOptions.LogMessageTemplate)
            .WriteTo.Logger(l => l
                .WriteTo.File(
                    Path.Combine(loggingOptions.LogsLocation, loggingOptions.LogFile),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: loggingOptions.LogMessageTemplate));

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}