using System;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.FuncApp;
using Serilog;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace RX.Nyss.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddConfiguration();
            builder.AddSerilog();
        }
    }

    public static class FunctionHostBuilderExtensions
    {
        private const string LocalSettingsJsonFileName = "local.settings.json";

        public static void AddConfiguration(this IFunctionsHostBuilder builder)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var localSettingsFile = Path.Combine(currentDirectory, LocalSettingsJsonFileName);

            var provider = builder.Services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();

            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile(localSettingsFile, optional: true, reloadOnChange: true)
                .AddConfiguration(configuration);
            
            var newConfiguration = configurationBuilder.Build();

            builder.Services.AddSingleton<IConfiguration>(newConfiguration);
        }

        public static void AddSerilog(this IFunctionsHostBuilder builder)
        {
            var provider = builder.Services.BuildServiceProvider();
            var configuration = provider.GetService<IConfiguration>();
            var instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

            if (!Enum.TryParse(configuration["LogLevel"], true, out LogEventLevel minimumEventLevel))
            {
                minimumEventLevel = LogEventLevel.Information;
            }

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(minimumEventLevel)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                loggerConfiguration = loggerConfiguration.WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces);
            }

            var logger = loggerConfiguration.CreateLogger();

            builder.Services.AddSingleton<ILogger>(logger);
        }
    }
}
