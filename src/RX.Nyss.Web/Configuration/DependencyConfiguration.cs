using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
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
            RegisterAuth(serviceCollection, config.Authentication);
            RegisterWebFramework(serviceCollection);
            if (!config.IsProduction)
            {
                RegisterSwagger(serviceCollection);
            }

            RegisterServiceCollection(serviceCollection, config);
        }

        private static void RegisterLogger(IServiceCollection serviceCollection,
            NyssConfig.LoggingOptions loggingOptions, IConfiguration configuration)
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

        private static void RegisterDatabases(IServiceCollection serviceCollection, NyssConfig.ConnectionStringOptions connectionStringOptions)
        {
            serviceCollection.AddDbContext<NyssContext>(options =>
                options.UseSqlServer(connectionStringOptions.NyssDatabase,
                    x => x.UseNetTopologySuite()));

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionStringOptions.NyssDatabase));
        }

        private static void RegisterAuth(IServiceCollection serviceCollection, NyssConfig.AuthenticationOptions authenticationOptions)
        {
            serviceCollection.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

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

            serviceCollection.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = authenticationOptions.Audience,
                        ValidIssuer = authenticationOptions.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.Secret))
                    };
                });
            
            RegisterAuthorizationPolicies(serviceCollection);

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

        private static void RegisterAuthorizationPolicies(IServiceCollection serviceCollection)
        {
            //ToDo: make some kind of automatic  registration of policies for all requirements in the assembly
            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.NationalSocietyAccess.ToString(),
                    policy => policy.Requirements.Add(new NationalSocietyAccessRequirement()));

                options.AddPolicy(Policy.ManagerAccess.ToString(),
                    policy => policy.Requirements.Add(new ManagerAccessRequirement()));

                options.AddPolicy(Policy.DataConsumerAccess.ToString(),
                    policy => policy.Requirements.Add(new DataConsumerAccessRequirement()));

                options.AddPolicy(Policy.TechnicalAdvisorAccess.ToString(),
                    policy => policy.Requirements.Add(new TechnicalAdvisorAccessRequirement()));
                    
                options.AddPolicy(Policy.SmsGatewayAccess.ToString(),
                    policy => policy.Requirements.Add(new SmsGatewayAccessRequirement()));

                options.AddPolicy(Policy.SupervisorAccess.ToString(),
                    policy => policy.Requirements.Add(new SupervisorAccessRequirement()));

                options.AddPolicy(Policy.DataCollectorAccess.ToString(),
                    policy => policy.Requirements.Add(new DataCollectorAccessRequirement()));

                options.AddPolicy(Policy.ProjectAccess.ToString(),
                    policy => policy.Requirements.Add(new ProjectAccessRequirement()));

                options.AddPolicy(Policy.HeadManagerAccess.ToString(),
                    policy => policy.Requirements.Add(new HeadManagerAccessHandlerRequirement()));

                options.AddPolicy(Policy.RegionAccess.ToString(),
                    policy => policy.Requirements.Add(new RegionAccessRequirement()));

                options.AddPolicy(Policy.DistrictAccess.ToString(),
                    policy => policy.Requirements.Add(new DistrictAccessRequirement()));

                options.AddPolicy(Policy.VillageAccess.ToString(),
                    policy => policy.Requirements.Add(new VillageAccessRequirement()));

                options.AddPolicy(Policy.ZoneAccess.ToString(),
                    policy => policy.Requirements.Add(new ZoneAccessRequirement()));

                options.AddPolicy(Policy.AlertAccess.ToString(),
                    policy => policy.Requirements.Add(new AlertAccessRequirement()));

            });

            serviceCollection.AddScoped<IAuthorizationHandler, NationalSocietyAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ManagerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DataConsumerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, TechnicalAdvisorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, SmsGatewayAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, SupervisorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DataCollectorAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, HeadManagerAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ProjectAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, RegionAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, DistrictAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, VillageAccessHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, ZoneAccessHandler>();
        }

        private static void RegisterWebFramework(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddControllersWithViews()
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

                        return new OkObjectResult(Result.Error(ResultKey.Validation.ValidationError, validationErrors));
                    };
                });

            serviceCollection.AddRazorPages();
            serviceCollection.AddHttpClient();

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

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization", 
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, 
                        new string[] { }
                    }
                });
            });

        private static void RegisterServiceCollection(IServiceCollection serviceCollection, NyssConfig config)
        {
            serviceCollection.AddSingleton<IConfig>(config);
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
