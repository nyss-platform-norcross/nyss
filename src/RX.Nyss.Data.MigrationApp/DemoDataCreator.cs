using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Data.MigrationApp
{
    public class DemoDataCreator
    {
        public static void CreateDemoData(string dbConnectionString, string password = "P@ssw0rd")
        {
            var usersToCreate = new (string roleName, string id, string name, string phone)[]
            {
                ("DataConsumer", "6be5c384-6675-a7be-4c45-3cba40f7cd13", "Jane Dataconsumer", "+1234561"),
                ("GlobalCoordinator", "5f259e96-4200-0cbc-4fe6-90024dcb770a", "Kim Globalconsumer", "+1234562"),
                ("Supervisor", "a076dd25-30d5-cbb2-4174-f73fda1524fa", "Bob Supervisor", "+1234563"),
                ("TechnicalAdvisor", "7c2d7baf-4400-6baf-497a-997a7ca18597", "Todd TechnicalAdvisor", "+1234564"),
                ("Manager", "9cfad185-f353-7c8f-4cf7-b2d3498eb715", "Mary Manager", "+1234565")
            };

            SeedIdentityUsers(dbConnectionString, usersToCreate, password);
            SeedNyssDemoData(dbConnectionString, usersToCreate);
        }

        private static void SeedIdentityUsers(string dbConnectionString, (string roleName, string id, string name, string phone)[] usersToCreate, string password)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            var identityUsers = usersToCreate.Select(user => new IdentityUser
            {
                Id = user.id,
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Email = $"{user.roleName}@example.com".ToLower(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = null,
                NormalizedEmail = $"{user.roleName}@example.com".ToUpper(),
                NormalizedUserName = $"{user.roleName}@example.com".ToUpper(),
                PasswordHash = hasher.HashPassword(null, password),
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = false,
                UserName = $"{user.roleName}@example.com".ToLower()
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

        private static void SeedNyssDemoData(string dbConnectionString, (string roleName, string id, string name, string phone)[] usersToCreate)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NyssContext>();
            optionsBuilder.UseSqlServer(dbConnectionString, x => x.UseNetTopologySuite());

            using (var context = new NyssContext(optionsBuilder.Options))
            {
                if (context.NationalSocieties.Any())
                {
                    throw new Exception("Database is not empty, existing national societies!");
                }

                context.NationalSocieties.Add(new NationalSociety
                {
                    ContentLanguage = context.ContentLanguages.First(),
                    Country = new Country
                    {
                        Name = "Mandawi",
                        CountryCode = "MI"
                    },
                    Name = "Mandawi example National Society",
                    StartDate = DateTime.UtcNow
                });
                context.SaveChanges();

                context.Projects.Add(new Project
                {
                    Name = "Mandawi test project",
                    NationalSociety = context.NationalSocieties.First(),
                    StartDate = DateTime.UtcNow,
                    TimeZone = "W. Europe Standard Time",
                    State = ProjectState.Open
                });
                context.SaveChanges();

                context.Users.AddRange(usersToCreate.Select(user =>
                {
                    User nyssUser = null;
                    var (roleName, id, name, phone) = user;

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
                    nyssUser.EmailAddress = $"{roleName}@example.com".ToLower();
                    nyssUser.IsFirstLogin = false;

                    return nyssUser;
                }));

                context.SaveChanges();

                context.SupervisorUserProjects.Add(new SupervisorUserProject
                {
                    Project = context.Projects.First(),
                    SupervisorUser = (SupervisorUser)context.Users.Single(u => u.Role == Role.Supervisor)
                });

                context.UserNationalSocieties.AddRange(usersToCreate.Where(x => x.roleName != "GlobalCoordinator").Select(user => new UserNationalSociety
                {
                    NationalSocietyId = 1,
                    UserId = context.Users.First(u => u.Name == user.name).Id
                }));

                context.SaveChanges();
            }

            Console.WriteLine("Successfully added NyssContext demo data");
        }
    }
}
