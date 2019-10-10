using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.User;

namespace RX.Nyss.Web.Features.Authorization
{
    public static class UserSeed
    {
        private const string SystemAdministratorEmail = "admin@domain.com";
        private const string SystemAdministratorPasswordConfigKey = "SystemAdministratorPassword";

        public static async Task SeedAdministratorAccount(this IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            //var loggerAdapter = serviceProvider.GetService<ILoggerAdapter>();

            //loggerAdapter.Debug("Seeding System Administrator account to the database.");

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var userService = serviceProvider.GetRequiredService<IUserService>();

            try
            {
                await userService.EnsureRoleExists(Role.SystemAdministrator.ToString());
                await userService.EnsureRoleExists(Role.GlobalCoordinator.ToString());
                await userService.EnsureRoleExists(Role.TechnicalAdvisor.ToString());
                await userService.EnsureRoleExists(Role.DataConsumer.ToString());
                await userService.EnsureRoleExists(Role.DataManager.ToString());
                await userService.EnsureRoleExists(Role.Supervisor.ToString());

                var systemAdministratorPassword = config[SystemAdministratorPasswordConfigKey];

                await userService.AddUser(SystemAdministratorEmail,systemAdministratorPassword, true);
                await userService.AssignRole(SystemAdministratorEmail, Role.SystemAdministrator.ToString());
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
