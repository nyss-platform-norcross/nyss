using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Data;
using RX.Nyss.Web.Models;

namespace RX.Nyss.Web.Features.Authorization
{
    public static class UserSeed
    {
        private const string SystemAdministratorUserName = "admin@fabres.pl";
        private const string SystemAdministratorPasswordConfigKey = "AdministratorPassword";

        public static async Task SeedAdministratorAccount(this IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            //var loggerAdapter = serviceProvider.GetService<ILoggerAdapter>();

            //loggerAdapter.Debug("Seeding System Administrator account to the database.");

            var config = serviceProvider.GetRequiredService<IConfiguration>();

            await using var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                //await EnsureRoleExists(serviceProvider, Role.SystemAdministrator.ToString());

                var systemAdministratorPassword = config[SystemAdministratorPasswordConfigKey];
                //await EnsureUserWithRoleExists(serviceProvider, SystemAdministratorUserName, systemAdministratorPassword, Role.SystemAdministrator.ToString(), true);

                //SeedDB(dbContext, systemAdministratorId);
            }
            catch (Exception e)
            {
                //loggerAdapter.Error($"Error occurred during seeding of System Administrator account.{Environment.NewLine}{e.Message}{Environment.NewLine}{e.StackTrace}");
                throw;
            }

            //loggerAdapter.Debug("System Administrator account was seeded successfully.");
        }
    }
}
