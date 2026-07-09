using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RX.Nyss.ReportFuncApp.Configuration;

namespace RX.Nyss.ReportFuncApp;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
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

                // Application Insights
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                // ✅ Bind & validate configuration
                var nyssFuncAppConfig = configuration.Get<NyssReportFuncAppConfig>()
                    ?? throw new InvalidOperationException(
                        "NyssReportFuncAppConfig is not configured");

                services.AddSingleton<IConfig>(nyssFuncAppConfig);

                // ✅ Azure Blob Storage (standard Functions storage)
                services.AddAzureClients(builder =>
                    builder.AddBlobServiceClient(configuration["AzureWebJobsStorage"])
                );

                // HTTP Client
                services.AddHttpClient();
            })
            .Build();

        host.Run();
    }
}
