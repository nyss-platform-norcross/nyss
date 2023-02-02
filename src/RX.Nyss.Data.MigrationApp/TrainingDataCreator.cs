using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;
using NyssUser = RX.Nyss.Data.Models.User;

namespace RX.Nyss.Data.MigrationApp
{
    public class TrainingDataCreator
    {
        private class User
        {
            public string RoleName { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public int OrganizationId { get; set; }
        }
        public static void CreateTrainingData(string dbConnectionString, int groupCount, string password = "nysstraining")
        {
            var usersToCreate = new User[groupCount];

            for (var i = 0; i < groupCount; i++)
            {
                usersToCreate[i] = new User
                {
                    RoleName = "GlobalCoordinator",
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Global Coordinator {i+1}",
                    Phone = $"+123456{i+1}",
                    OrganizationId =  1
                };
            }

            SeedIdentityUsers(dbConnectionString, usersToCreate, password);
            SeedTrainingData(dbConnectionString, groupCount, usersToCreate);
        }

        private static void SeedIdentityUsers(string dbConnectionString, User[] usersToCreate, string password)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            var identityUsers = usersToCreate.Select(user => new IdentityUser
            {
                Id = user.Id,
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Email = $"{user.Name.Replace(" ", "_")}@example.com".ToLower(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = null,
                NormalizedEmail = $"{user.Name.Replace(" ", "_")}@example.com".ToUpper(),
                NormalizedUserName = $"{user.Name.Replace(" ", "_")}@example.com".ToUpper(),
                PasswordHash = hasher.HashPassword(null, password),
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                UserName = $"{user.Name.Replace(" ", "_")}@example.com".ToLower()
            });

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                if (context.Users.Count() > 1)
                {
                    throw new Exception("Database is not empty! More than just the admin user exists.");
                }

                context.Users.AddRange(identityUsers);
                context.SaveChanges();

                context.UserRoles
                    .AddRange(usersToCreate
                        .Select(user =>
                            new IdentityUserRole<string>
                            {
                                RoleId = context.Roles.First(r => r.Name == user.RoleName).Id,
                                UserId = user.Id
                            }));
                context.SaveChanges();
            }

            Console.WriteLine("Successfully added ApplicationDbContext demo data");
        }

        private static void SeedTrainingData(string dbConnectionString, int numberOfGroups, IEnumerable<User> usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, options =>
            {
                options.UseNetTopologySuite();
            });

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
                    NyssUser nyssUser = user.RoleName switch
                    {
                        "Supervisor" => new SupervisorUser
                        {
                            Role = Role.Supervisor,
                            CurrentProject = context.Projects.First(),
                            Sex = Sex.Other,
                            DecadeOfBirth = 1980
                        },
                        "HeadSupervisor" => new HeadSupervisorUser
                        {
                            Role = Role.Supervisor,
                            CurrentProject = context.Projects.First(),
                            Sex = Sex.Other,
                            DecadeOfBirth = 1990
                        },
                        "DataConsumer" => new DataConsumerUser(),
                        "GlobalCoordinator" => new GlobalCoordinatorUser(),
                        "Coordinator" => new CoordinatorUser(),
                        "Manager" => new ManagerUser(),
                        "TechnicalAdvisor" => new TechnicalAdvisorUser(),
                        _ => null
                    };

                    nyssUser.Name = $"{user.Name}";
                    nyssUser.PhoneNumber = user.Phone;
                    nyssUser.IdentityUserId = user.Id;
                    nyssUser.ApplicationLanguage = context.ApplicationLanguages.First();
                    nyssUser.EmailAddress = $"{user.Name.Replace(" ", "_")}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;
                    return nyssUser;
                }));
                context.SaveChanges();
            }
            Console.WriteLine("Successfully added NyssContext training data");
        }
    }
}
