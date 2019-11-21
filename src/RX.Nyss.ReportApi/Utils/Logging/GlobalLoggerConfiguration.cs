using System.IO;
using RX.Nyss.ReportApi.Configuration;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.ReportApi.Utils.Logging
{
    public static class GlobalLoggerConfiguration
    {
        public static void ConfigureLogger(NyssReportApiConfig.LoggingOptions loggingOptions, string appInsightsInstrumentationKey)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: loggingOptions.LogMessageTemplate)
                .WriteTo.Logger(l => l
                    .WriteTo.File(
                        Path.Combine(loggingOptions.LogsLocation, "nyss-report-api-logs-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: loggingOptions.LogMessageTemplate));

            if (!string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                loggerConfiguration.WriteTo.ApplicationInsights(appInsightsInstrumentationKey, TelemetryConverter.Traces);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
