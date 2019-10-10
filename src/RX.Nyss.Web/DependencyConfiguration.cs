using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;
using RX.Nyss.Web.Features.HealthRisk;
using RX.Nyss.Web.Features.Logging;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Models;
using Serilog;

namespace RX.Nyss.Web
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDependencies(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var config = configuration.Get<NyssConfig>();
            GlobalLoggerConfiguration.ConfigureLogger(config.Logging);
            RegisterLogger(serviceCollection);
            RegisterDatabases(serviceCollection, configuration);
            RegisterAuth(serviceCollection);
            RegisterWebFramework(serviceCollection);
            RegisterSwagger(serviceCollection);
            RegisterServiceCollection(serviceCollection);
        }

        private static void RegisterLogger(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(x => Log.Logger); // must be func, as the static logger is configured (changed reference) after DI registering
            serviceCollection.AddSingleton<ILoggerAdapter, SerilogLoggerAdapter>();
        }

        private static void RegisterDatabases(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<NyssContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("NyssDatabase"),
                    x => x.UseNetTopologySuite()));

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("NyssDatabase")));
        }

        private static void RegisterAuth(IServiceCollection serviceCollection)
        {
            serviceCollection.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            serviceCollection.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            serviceCollection.AddAuthentication()
                .AddIdentityServerJwt();
        }

        private static void RegisterWebFramework(IServiceCollection serviceCollection)
        {
            serviceCollection.AddControllersWithViews();
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
        }
    }
}
