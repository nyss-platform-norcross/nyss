using System.IO;
using RX.Nyss.Web.Configuration;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class GlobalLoggerConfiguration
    {
        public static void ConfigureLogger(NyssConfig.ILoggingOptions loggingOptions, string appInsightsInstrumentationKey)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: loggingOptions.LogMessageTemplate)
                .WriteTo.Logger(l => l
                    .WriteTo.File(
                        Path.Combine(loggingOptions.LogsLocation, "nyss-logs-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: loggingOptions.LogMessageTemplate));

            if (appInsightsInstrumentationKey != null)
            {
                loggerConfiguration.WriteTo.ApplicationInsights(appInsightsInstrumentationKey, TelemetryConverter.Traces);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
