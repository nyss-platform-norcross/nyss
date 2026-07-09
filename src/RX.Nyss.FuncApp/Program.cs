using System.IO;
using System.Reflection;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp;

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

                // Bind configuration
                var nyssFuncAppConfig = configuration.Get<NyssFuncAppConfig>()
                    ?? throw new InvalidOperationException("NyssFuncAppConfig is not configured");
                services.AddSingleton<IConfig>(nyssFuncAppConfig);

                //  Azure Blob Storage 
                services.AddAzureClients(builder =>
                    builder.AddBlobServiceClient(
                        configuration["AzureWebJobsStorage"]));
                services.AddAzureClients(builder =>
                    builder.AddBlobServiceClient(
                        configuration["GeneralBlobContainer"]));
                services.AddAzureClients(builder =>
                    builder.AddBlobServiceClient(
                        configuration["SmsGatewayBlobContainer"]));

                // Azure Service Bus
                services.AddAzureClients(builder =>
                    builder.AddServiceBusClient(
                        configuration["SERVICEBUS_CONNECTIONSTRING"]));

                // HTTP
                services.AddHttpClient();
                services.AddSingleton<IHttpPostClient, HttpPostClient>();

                // Application Services
                services.AddScoped<IEmailService, EmailService>();
                services.AddScoped<IWhitelistValidator, WhitelistValidator>();
                services.AddScoped<ISmsService, SmsService>();
                services.AddScoped<IEmailAttachmentService, EmailAttachmentService>();
                services.AddScoped<IDeadLetterSmsService, DeadLetterSmsService>();
                services.AddScoped<IReportPublisherService, ReportPublisherService>();
                services.AddScoped<ITelerivetReportPublisherService, TelerivetReportPublisherService>();
                services.AddScoped<IMTNReportPublisherService, MTNReportPublisherService>();

                // Email Client (Dev vs Prod)
                services.AddScoped(typeof(IEmailClient),
                    configuration["AZURE_FUNCTIONS_ENVIRONMENT"] == "Development"
                        ? typeof(DummyConsoleEmailClient)
                        : typeof(SendGridEmailClient));
            })
            .Build();

        host.Run();
    }
}
