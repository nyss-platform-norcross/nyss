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

            using (var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                try
                {
                    await EnsureRoleExists(serviceProvider, Role.SystemAdministrator.ToString());

                    var systemAdministratorPassword = config[SystemAdministratorPasswordConfigKey];
                    await EnsureUserWithRoleExists(serviceProvider, SystemAdministratorUserName, systemAdministratorPassword, Role.SystemAdministrator.ToString());

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

        private static async Task EnsureRoleExists(IServiceProvider serviceProvider, string role)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleCreationResult = await roleManager.CreateAsync(new IdentityRole(role));

                if (!roleCreationResult.Succeeded)
                {
                    var errorMessages = string.Join(",", roleCreationResult.Errors.Select(x => x.Description));
                    throw new Exception($"The System Administrator role could not be created. {errorMessages}");
                }
            }
        }

        private static async Task EnsureUserWithRoleExists(IServiceProvider serviceProvider, string userName, string userPassword, string userRole)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(userName);

            if (user != null)
            {
                return;
            }
            
            user = new ApplicationUser
            {
                UserName = userName,
                EmailConfirmed = true
            };

            var userCreationResult = await userManager.CreateAsync(user, userPassword);

            if (!userCreationResult.Succeeded)
            {
                var errorMessages = string.Join(",", userCreationResult.Errors.Select(x => x.Description));
                throw new Exception($"The System Administrator user could not be created. {errorMessages}");
            }

            var assignmentToRoleResult = await userManager.AddToRoleAsync(user, userRole);

            if (!assignmentToRoleResult.Succeeded)
            {
                var errorMessages = string.Join(",", assignmentToRoleResult.Errors.Select(x => x.Description));
                throw new Exception($"The System Administrator role could not be assigned. {errorMessages}");
            }
        }
    }
}
