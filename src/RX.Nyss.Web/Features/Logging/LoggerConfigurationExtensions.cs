using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithBasicConfiguration(this LoggerConfiguration loggerConfiguration) =>
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithCaller()
                // Console (stdout) grabs and writes down everything
                .WriteTo.Console(outputTemplate: LoggerConfigurationTemplates.ExceptionTemplate);

        public static LoggerConfiguration WithLoggingExceptions(this LoggerConfiguration loggerConfiguration, string logsLocation) =>
            loggerConfiguration
                .WriteTo.Logger(l => l
                    .MinimumLevel.Error()
                    .WriteTo.File(
                        Path.Combine(logsLocation, "errors-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: LoggerConfigurationTemplates.ExceptionTemplate));

        public static LoggerConfiguration WithStandardLogging(this LoggerConfiguration loggerConfiguration,
            string logsLocation, List<Func<LogEvent, bool>> exclusionEvents) =>
            loggerConfiguration.WriteTo.Logger(l => l
                .Filter.ByExcluding(le => le.Level == LogEventLevel.Error || le.Level == LogEventLevel.Fatal)
                .ApplyLoggingFilterExclusions(exclusionEvents)
                .WriteTo.File(
                    Path.Combine(logsLocation, "logs-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: LoggerConfigurationTemplates.CallerTemplate));

        private static LoggerConfiguration ApplyLoggingFilterExclusions(this LoggerConfiguration loggerConfiguration, IEnumerable<Func<LogEvent, bool>> exclusionEvents)
        {
            foreach (var exclusionEvent in exclusionEvents)
            {
                loggerConfiguration.Filter.ByExcluding(exclusionEvent);
            }

            return loggerConfiguration;
        }
    }
}
