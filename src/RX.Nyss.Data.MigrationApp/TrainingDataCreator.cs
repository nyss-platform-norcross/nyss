using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Data.MigrationApp
{
    public class TrainingDataCreator
    {
        public static void CreateTrainingData(string dbConnectionString, int groups, string password = "nysstraining")
        {
            var usersToCreate = new (string roleName, string id, string name, string phone, int organizationId)[groups];

            for (var i = 0; i < groups; i++)
            {
                usersToCreate[i] = ("GlobalCoordinator", Guid.NewGuid().ToString(), $"Global Coordinator {i+1}", $"+123456{i+1}", 1);
            }

            SeedIdentityUsers(dbConnectionString, usersToCreate, password);
            SeedTrainingData(dbConnectionString, groups, usersToCreate);
        }

        private static void SeedIdentityUsers(string dbConnectionString, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate, string password)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            var identityUsers = usersToCreate.Select(user => new IdentityUser
            {
                Id = user.id,
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Email = $"{user.name.Replace(" ", "_")}@example.com".ToLower(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = null,
                NormalizedEmail = $"{user.name.Replace(" ", "_")}@example.com".ToUpper(),
                NormalizedUserName = $"{user.name.Replace(" ", "_")}@example.com".ToUpper(),
                PasswordHash = hasher.HashPassword(null, password),
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                UserName = $"{user.name.Replace(" ", "_")}@example.com".ToLower()
            });

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                if (context.Users.Count() > 1)
                {
                    throw new Exception("Database is not empty! More than just the admin user exists.");
                }

                context.Users.AddRange(identityUsers);
                context.SaveChanges();
                context.UserRoles.AddRange(usersToCreate.Select(user => new IdentityUserRole<string>
                {
                    RoleId = context.Roles.First(r => r.Name == user.roleName).Id,
                    UserId = user.id
                }));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added ApplicationDbContext demo data");
        }

        private static void SeedTrainingData(string dbConnectionString, int numberOfGroups, (string roleName, string id, string name, string phone, int organizationId)[] usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                if (context.NationalSocieties.Any())
                {
                    throw new Exception("Database is not empty, existing national societies!");
                }

                var groups = Enumerable.Range(1, numberOfGroups);
                var nationalSocieties = groups.Select(i => new NationalSociety
                {
                    ContentLanguage = context.ContentLanguages.First(),
                    Country = new Country
                    {
                        Name = "Mandawi",
                        CountryCode = "MI"
                    },
                    Name = $"Training group {i}",
                    StartDate = DateTime.UtcNow,
                    Organizations = new List<Organization>
                    {
                        new Organization { Name = "Demo Organization" }
                    }
                });

                context.NationalSocieties.AddRange(nationalSocieties);
                context.SaveChanges();

                foreach (var ns in context.NationalSocieties.ToList())
                {
                    ns.DefaultOrganization = ns.Organizations.First();
                }

                context.Users.AddRange(usersToCreate.Select(user =>
                {
                    User nyssUser = null;
                    var (roleName, id, name, phone, organizationId) = user;

                    switch (roleName)
                    {
                        case "Supervisor":
                            nyssUser = new SupervisorUser
                            {
                                Role = Role.Supervisor,
                                CurrentProject = context.Projects.First(),
                                Sex = Sex.Other,
                                DecadeOfBirth = 1980
                            };
                            break;
                        case "DataConsumer":
                            nyssUser = new DataConsumerUser();
                            break;
                        case "GlobalCoordinator":
                            nyssUser = new GlobalCoordinatorUser();
                            break;
                        case "Coordinator":
                            nyssUser = new CoordinatorUser();
                            break;
                        case "Manager":
                            nyssUser = new ManagerUser();
                            break;
                        case "TechnicalAdvisor":
                            nyssUser = new TechnicalAdvisorUser();
                            break;
                    }

                    nyssUser.Name = $"{name}";
                    nyssUser.PhoneNumber = phone;
                    nyssUser.IdentityUserId = id;
                    nyssUser.ApplicationLanguage = context.ApplicationLanguages.First();
                    nyssUser.EmailAddress = $"{user.name.Replace(" ", "_")}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;

                    return nyssUser;
                }));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added NyssContext training data");
        }
    }
}
