﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;
using RX.Nyss.Web.Features.HealthRisk;
using RX.Nyss.Web.Features.Logging;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.MultiContextTransactions;
using Serilog;

namespace RX.Nyss.Web.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = configuration.Get<NyssConfig>();
            RegisterLogger(serviceCollection, config.Logging, configuration);
            RegisterDatabases(serviceCollection, config.ConnectionStrings);
            RegisterAuth(serviceCollection);
            RegisterWebFramework(serviceCollection);
            RegisterSwagger(serviceCollection);
            RegisterServiceCollection(serviceCollection);
        }

        private static void RegisterLogger(IServiceCollection serviceCollection,
            NyssConfig.ILoggingOptions loggingOptions, IConfiguration configuration)
        {
            const string applicationInsightsEnvironmentVariable = "APPINSIGHTS_INSTRUMENTATIONKEY";
            var appInsightsInstrumentationKey = configuration[applicationInsightsEnvironmentVariable];
            GlobalLoggerConfiguration.ConfigureLogger(loggingOptions, appInsightsInstrumentationKey);
            serviceCollection.AddSingleton(x => Log.Logger); // must be func, as the static logger is configured (changed reference) after DI registering
            serviceCollection.AddSingleton<ILoggerAdapter, SerilogLoggerAdapter>();

            serviceCollection.AddApplicationInsightsTelemetry();
        }

        private static void RegisterDatabases(IServiceCollection serviceCollection, NyssConfig.IConnectionStringOptions connectionStringOptions)
        {
            var databaseConnection = new SqlConnection(connectionStringOptions.NyssDatabase);

            serviceCollection.AddDbContext<NyssContext>(options =>
                options.UseSqlServer(databaseConnection,
                    x => x.UseNetTopologySuite()));

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(databaseConnection));
        }

        private static void RegisterAuth(IServiceCollection serviceCollection)
        {
            serviceCollection.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            serviceCollection.AddIdentityServer()
                .AddApiAuthorization<IdentityUser, ApplicationDbContext>();

            serviceCollection.AddAuthentication()
                .AddIdentityServerJwt();

            serviceCollection.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    if (IsAjaxRequest(context.Request) ||
                        IsApiRequest(context.Request))
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            bool IsApiRequest(HttpRequest request) => request.Path.StartsWithSegments("/api");

            bool IsAjaxRequest(HttpRequest request) =>
                string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }

        private static void RegisterWebFramework(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddControllersWithViews()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext =>
                    {
                        var validationErrors = actionContext.ModelState.Where(v => v.Value.Errors.Count > 0)
                            .ToDictionary(stateEntry => stateEntry.Key,
                                stateEntry => stateEntry.Value.Errors.Select(x => x.ErrorMessage));

                        return new OkObjectResult(Result.Error(ResultKey.Validation.ValidationError, validationErrors));
                    };
                });

            serviceCollection.AddRazorPages();

            // In production, the React files will be served from this directory
            serviceCollection.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        private static void RegisterSwagger(IServiceCollection serviceCollection) =>
            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nyss API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        public static void RegisterServiceCollection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IHealthRiskService, HealthRiskService>();
            serviceCollection.AddScoped<IUserService, UserService>();
            serviceCollection.AddScoped<INyssContext>(x => x.GetService<NyssContext>());
            serviceCollection.AddScoped<IDbTransactionFactory, DbTransactionFactory>();
        }
    }
}