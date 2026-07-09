using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RX.Nyss.PublicApiFuncApp.Configuration;

namespace RX.Nyss.PublicApiFuncApp;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var localSettingsFile = Path.Combine(currentDirectory, "local.settings.json");

                config
                    .AddJsonFile(localSettingsFile, optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Application Insights for Azure Functions
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                // Bind configuration
                var nyssPublicApiFuncAppConfig = configuration.Get<NyssPublicApiFuncAppConfig>();
                services.AddSingleton<IConfig>(nyssPublicApiFuncAppConfig);
            })
            .Build();

        host.Run();
    }
}