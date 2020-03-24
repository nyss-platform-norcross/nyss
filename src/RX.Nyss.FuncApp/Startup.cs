using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.FuncApp;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Services;

[assembly: FunctionsStartup(typeof(Startup))]

namespace RX.Nyss.FuncApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddConfiguration();
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
                .AddJsonFile(localSettingsFile, true, true)
                .AddEnvironmentVariables()
                .AddConfiguration(configuration)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), false);

            var newConfiguration = configurationBuilder.Build();
            var nyssFuncAppConfig = newConfiguration.Get<NyssFuncAppConfig>();
            builder.Services.AddSingleton<IConfiguration>(newConfiguration);
            builder.Services.AddSingleton<IConfig>(nyssFuncAppConfig);
            builder.Services.AddHttpClient();
            builder.Services.AddLogging();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddSingleton<IHttpPostClient, HttpPostClient>();
            
            if (nyssFuncAppConfig.MailConfig.UseSendGrid)
            {
                builder.Services.AddSingleton<IEmailClient, SendGridEmailClient>();
            }
            else
            {
                builder.Services.AddSingleton<IEmailClient, MailjetEmailClient>();
            }
        }
    }
}
