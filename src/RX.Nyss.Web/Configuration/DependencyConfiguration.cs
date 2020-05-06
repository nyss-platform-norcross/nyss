using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.OpenApi.Models;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;
using RX.Nyss.Web.Features.Alerts.Access;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Coordinators.Access;
using RX.Nyss.Web.Features.DataCollectors.Access;
using RX.Nyss.Web.Features.DataConsumers.Access;
using RX.Nyss.Web.Features.Managers.Access;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocietyStructure.Access;
using RX.Nyss.Web.Features.Organizations.Access;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Features.Reports.Access;
using RX.Nyss.Web.Features.SmsGateways.Access;
using RX.Nyss.Web.Features.Supervisors.Access;
using RX.Nyss.Web.Features.TechnicalAdvisors.Access;
using RX.Nyss.Web.Features.Users.Access;
using RX.Nyss.Web.Utils;
using Serilog;

namespace RX.Nyss.Web.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = configuration.Get<ConfigSingleton>();
            RegisterLogger(serviceCollection, config.Logging, configuration);
            RegisterDatabases(serviceCollection, config.ConnectionStrings);
            RegisterAuth(serviceCollection, config.Authentication);
            RegisterWebFramework(serviceCollection);
            if (!config.IsProduction)
            {
                RegisterSwagger(serviceCollection);
            }

            RegisterServiceCollection(serviceCollection, config);
        }

        private static void RegisterLogger(IServiceCollection serviceCollection,
            ILoggingOptions loggingOptions, IConfiguration configuration)
        {
            const string applicationInsightsEnvironmentVariable = "APPINSIGHTS_INSTRUMENTATIONKEY";
            var appInsightsInstrumentationKey = configuration[applicationInsightsEnvironmentVariable];
            GlobalLoggerConfiguration.ConfigureLogger(loggingOptions, appInsightsInstrumentationKey);
            serviceCollection.AddSingleton(x => Log.Logger); // must be func, as the static logger is configured (changed reference) after DI registering
            serviceCollection.AddSingleton<ILoggerAdapter, SerilogLoggerAdapter>();

            if (!string.IsNullOrEmpty(appInsightsInstrumentationKey))
            {
                serviceCollection.AddApplicationInsightsTelemetry();
            }
        }

        private static void RegisterDatabases(IServiceCollection serviceCollection, IConnectionStringOptions connectionStringOptions)
        {
            serviceCollection.AddDbContext<NyssContext>(options =>
                options.UseSqlServer(connectionStringOptions.NyssDatabase,
                    x => x.UseNetTopologySuite()));

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionStringOptions.NyssDatabase));
        }

        private static void RegisterAuth(IServiceCollection serviceCollection, ConfigSingleton.AuthenticationOptions authenticationOptions)
        {
            serviceCollection
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Tokens.EmailConfirmationTokenProvider = "Email";
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddEmailTokenVerificationProvider();

            serviceCollection
                .AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>();

            serviceCollection.Configure<DataProtectionTokenProviderOptions>(o =>
            {
                o.Name = "Default";
                o.TokenLifespan = TimeSpan.FromMinutes(30);
            });

            serviceCollection.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            });

            RegisterAuthorizationPolicies(serviceCollection);

            serviceCollection.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = authenticationOptions.CookieName;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.CookieExpirationTime);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    if (IsAjaxRequest(context.Request) || IsApiRequest(context.Request))
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (IsAjaxRequest(context.Request) || IsApiRequest(context.Request))
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            static bool IsApiRequest(HttpRequest request) =>
                request.Path.StartsWithSegments("/api");

            static bool IsAjaxRequest(HttpRequest request) =>
                string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }

        private static void RegisterAuthorizationPolicies(IServiceCollection serviceCollection)
        {
            //ToDo: make some kind of automatic  registration of policies for all requirements in the assembly
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.NationalSocietyAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<NationalSocietyAccessHandler>.Requirement()));

                options.AddPolicy(Policy.ManagerAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<ManagerAccessHandler>.Requirement()));

                options.AddPolicy(Policy.DataConsumerAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<DataConsumerAccessHandler>.Requirement()));

                options.AddPolicy(Policy.TechnicalAdvisorAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<TechnicalAdvisorAccessHandler>.Requirement()));

                options.AddPolicy(Policy.SmsGatewayAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<SmsGatewayAccessHandler>.Requirement()));

                options.AddPolicy(Policy.OrganizationAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<OrganizationAccessHandler>.Requirement()));

                options.AddPolicy(Policy.SupervisorAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<SupervisorAccessHandler>.Requirement()));

                options.AddPolicy(Policy.DataCollectorAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<DataCollectorAccessHandler>.Requirement()));

                options.AddPolicy(Policy.MultipleDataCollectorsAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceMultipleAccessHandler<MultipleDataCollectorsAccessHandler>.Requirement()));

                options.AddPolicy(Policy.ProjectAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<ProjectAccessHandler>.Requirement()));

                options.AddPolicy(Policy.HeadManagerAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<HeadManagerAccessHandler>.Requirement()));

                options.AddPolicy(Policy.RegionAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<RegionAccessHandler>.Requirement()));

                options.AddPolicy(Policy.DistrictAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<DistrictAccessHandler>.Requirement()));

                options.AddPolicy(Policy.VillageAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<VillageAccessHandler>.Requirement()));

                options.AddPolicy(Policy.ZoneAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<ZoneAccessHandler>.Requirement()));

                options.AddPolicy(Policy.AlertAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<AlertAccessHandler>.Requirement()));

                options.AddPolicy(Policy.ReportAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<ProjectAccessHandler>.Requirement()));

                options.AddPolicy(Policy.CoordinatorAccess.ToString(),
                    policy => policy.Requirements.Add(new ResourceAccessHandler<CoordinatorAccessHandler>.Requirement()));
            });

            serviceCollection.AddScoped<IAuthorizationHandler, NationalSocietyAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ManagerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DataConsumerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, TechnicalAdvisorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, SmsGatewayAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, OrganizationAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, SupervisorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DataCollectorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, MultipleDataCollectorsAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, HeadManagerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ProjectAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, RegionAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DistrictAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, VillageAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ZoneAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, AlertAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ReportAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, CoordinatorAccessHandler>();
        }

        private static void RegisterWebFramework(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringDateTimeConverter());
                })
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
            serviceCollection.AddHttpClient();

            // In production, the React files will be served from this directory
            serviceCollection.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/build");
        }

        private static void RegisterSwagger(IServiceCollection serviceCollection) =>
            serviceCollection.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Nyss API",
                    Version = "v1"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        private static void RegisterServiceCollection(IServiceCollection serviceCollection, ConfigSingleton config)
        {
            serviceCollection.AddSingleton<IConfig>(config);
            serviceCollection.AddSingleton<INyssWebConfig>(config);
            RegisterTypes(serviceCollection, "RX.Nyss");
        }

        private static void RegisterTypes(IServiceCollection serviceCollection, string namePrefix) =>
            GetAssemblies(namePrefix: namePrefix)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Select(type => new
                {
                    implementationType = type,
                    interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}")
                })
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
