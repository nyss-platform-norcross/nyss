using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.OpenApi.Models;
using RX.Nyss.Data;
using RX.Nyss.ReportApi.Handlers;
using RX.Nyss.ReportApi.Utils.Logging;
using Serilog;

namespace RX.Nyss.ReportApi.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = configuration.Get<NyssReportApiConfig>();
            RegisterLogger(serviceCollection, config.Logging, configuration);
            RegisterDatabases(serviceCollection, config.ConnectionStrings);
            RegisterWebFramework(serviceCollection);
            RegisterSwagger(serviceCollection);
            RegisterServiceCollection(serviceCollection, config);
        }

        private static void RegisterLogger(IServiceCollection serviceCollection,
            NyssReportApiConfig.LoggingOptions loggingOptions, IConfiguration configuration)
        {
            const string applicationInsightsEnvironmentVariable = "APPINSIGHTS_INSTRUMENTATIONKEY";
            var appInsightsInstrumentationKey = configuration[applicationInsightsEnvironmentVariable];
            GlobalLoggerConfiguration.ConfigureLogger(loggingOptions, appInsightsInstrumentationKey);
            serviceCollection.AddSingleton(x => Log.Logger); // must be func, as the static logger is configured (changed reference) after DI registering
            serviceCollection.AddSingleton<ILoggerAdapter, SerilogLoggerAdapter>();

            if (!string.IsNullOrEmpty(applicationInsightsEnvironmentVariable))
            {
                serviceCollection.AddApplicationInsightsTelemetry();
            }
        }

        private static void RegisterDatabases(IServiceCollection serviceCollection, NyssReportApiConfig.ConnectionStringOptions connectionStringOptions) =>
            serviceCollection.AddDbContext<NyssContext>(options =>
                options.UseSqlServer(connectionStringOptions.NyssDatabase,
                    x => x.UseNetTopologySuite()));

        private static void RegisterWebFramework(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                })
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext =>
                    {
                        var validationErrors = actionContext.ModelState.Where(v => v.Value.Errors.Count > 0)
                            .ToDictionary(stateEntry => stateEntry.Key,
                                stateEntry => stateEntry.Value.Errors.Select(x => x.ErrorMessage));

                        return new BadRequestObjectResult(validationErrors);
                    };
                });

            serviceCollection.AddHttpClient();
        }

        private static void RegisterSwagger(IServiceCollection serviceCollection) =>
            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nyss Report API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        private static void RegisterServiceCollection(IServiceCollection serviceCollection, NyssReportApiConfig nyssReportApiConfig)
        {
            serviceCollection.AddSingleton<IConfig>(nyssReportApiConfig);
            serviceCollection.AddScoped<ISmsHandler, SmsEagleHandler>();
            RegisterTypes(serviceCollection, "RX.Nyss");
        }

        private static void RegisterTypes(IServiceCollection serviceCollection, string namePrefix) =>
            GetAssemblies(namePrefix: namePrefix)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => new { implementationType = type, interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}") })
                .Where(x => x.interfaceType != null)
                .ToList()
                .ForEach(i => serviceCollection.AddScoped(i.interfaceType, i.implementationType));

        private static Assembly[] GetAssemblies(string namePrefix) =>
            DependencyContext.Default.GetDefaultAssemblyNames()
                .Where(name => name.Name.StartsWith(namePrefix))
                .Select(Assembly.Load)
                .ToArray();
    }
}
