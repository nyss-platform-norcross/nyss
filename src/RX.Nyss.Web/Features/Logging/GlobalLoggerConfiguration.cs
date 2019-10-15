using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using RX.Nyss.Web.Configuration;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class GlobalLoggerConfiguration
    {
        public static void ConfigureLogger(NyssConfig.ILoggingOptions loggingOptions) =>
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: loggingOptions.LogMessageTemplate)
                .WriteTo.Logger(l => l
                    .WriteTo.File(
                        Path.Combine(loggingOptions.LogsLocation, "nyss-logs-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: loggingOptions.LogMessageTemplate))
                .WriteTo.ApplicationInsights(loggingOptions.ApplicationInsightsInstrumentationKey, TelemetryConverter.Traces)
                .CreateLogger();
    }
}
