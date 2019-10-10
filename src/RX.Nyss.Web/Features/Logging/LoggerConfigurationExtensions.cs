using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithBasicConnectorConfiguration(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithCaller()
                // Console (stdout) grabs and writes down everything
                .WriteTo.Console(outputTemplate: LoggerConfigurationTemplates.ExceptionTemplate);
        }

        public static LoggerConfiguration WithLoggingExceptions(this LoggerConfiguration loggerConfiguration, string logsLocation)
        {
            // errors logs for Error and Fatal messages
            return loggerConfiguration
                .WriteTo.Logger(l => l
                    .MinimumLevel.Error()
                    .WriteTo.File(
                        Path.Combine(logsLocation, "errors-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: LoggerConfigurationTemplates.ExceptionTemplate));
        }

        public static LoggerConfiguration WithLoggingRepositories(this LoggerConfiguration loggerConfiguration, string logsLocation)
        {
            return loggerConfiguration
                // Repo logs containing only called repository methods
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(LogEventChecks.IsRepositoryCall)
                    .WriteTo.File(
                        Path.Combine(logsLocation, "repo-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: LoggerConfigurationTemplates.CallerTemplate));
        }

        public static LoggerConfiguration WithLoggingMassTransit(this LoggerConfiguration loggerConfiguration, string logsLocation)
        {
            return loggerConfiguration
                .WriteTo.Logger(l => l
                    .Filter.ByIncludingOnly(LogEventChecks.IsMassTransit)
                    .WriteTo.File(
                        Path.Combine(logsLocation, "bus-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: LoggerConfigurationTemplates.CallerTemplate));
        }

        public static LoggerConfiguration WithStandardLogging(this LoggerConfiguration loggerConfiguration,
            string logsLocation, List<Func<LogEvent, bool>> exclusionEvents)
        {
            // standard logs including all usages of our logger without errors
            return loggerConfiguration.WriteTo.Logger(l => l
                .Filter.ByExcluding(le => le.Level == LogEventLevel.Error || le.Level == LogEventLevel.Fatal)
                .ApplyLoggingFilterExclusions(exclusionEvents)
                .WriteTo.File(
                    Path.Combine(logsLocation, "logs-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: LoggerConfigurationTemplates.CallerTemplate));
        }

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
