using System;
using System.Collections.Generic;
using RX.Nyss.Web.Configuration;
using Serilog;
using Serilog.Events;

namespace RX.Nyss.Web.Features.Logging
{
    public static class GlobalLoggerConfiguration
    {
        public static void ConfigureLogger(NyssConfig.ILoggingOptions loggingOptions) =>
            Log.Logger = new LoggerConfiguration()
                .WithBasicConnectorConfiguration()
                .WithLoggingExceptions(loggingOptions.LogsLocation)
                .WithLoggingRepositories(loggingOptions.LogsLocation)
                .WithLoggingMassTransit(loggingOptions.LogsLocation)
                .WithStandardLogging(loggingOptions.LogsLocation, new List<Func<LogEvent, bool>>
                {
                    LogEventChecks.IsRepositoryCall,
                    LogEventChecks.IsMassTransit
                })
                .CreateLogger();
    }
}
